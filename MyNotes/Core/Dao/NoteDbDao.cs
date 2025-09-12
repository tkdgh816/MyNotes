using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;

using Microsoft.Data.Sqlite;

using MyNotes.Common.Collections;
using MyNotes.Core.Dto;
using MyNotes.Core.Model;
using MyNotes.Core.Service;
using MyNotes.Core.Shared;

namespace MyNotes.Core.Dao;
internal class NoteDbDao(DatabaseService databaseService) : DbDaoBase
{
  private readonly DatabaseService _databaseService = databaseService;
  private readonly int _batchSize = 20;

  #region Create
  public bool CreateNote(NoteDto dto)
  {
    using SqliteConnection connection = _databaseService.Connection;
    connection.Open();
    string query = """
      INSERT OR IGNORE INTO Notes
      (id, parent, created, modified, title, body, preview, background, backdrop, width, height, position_x, position_y, bookmarked, trashed)
      VALUES
      (@id, @parent, @created, @modified, @title, @body, @preview, @background, @backdrop, @width, @height, @position_x, @position_y, @bookmarked, @trashed)
      """;
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@id", dto.Id);
    command.Parameters.AddWithValue("@parent", dto.Parent);
    command.Parameters.AddWithValue("@created", dto.Created);
    command.Parameters.AddWithValue("@modified", dto.Modified);
    command.Parameters.AddWithValue("@title", dto.Title);
    command.Parameters.AddWithValue("@body", dto.Body);
    command.Parameters.AddWithValue("@preview", dto.Preview);
    command.Parameters.AddWithValue("@background", dto.Background);
    command.Parameters.AddWithValue("@backdrop", dto.Backdrop);
    command.Parameters.AddWithValue("@width", dto.Width);
    command.Parameters.AddWithValue("@height", dto.Height);
    command.Parameters.AddWithValue("@position_x", dto.PositionX);
    command.Parameters.AddWithValue("@position_y", dto.PositionY);
    command.Parameters.AddWithValue("@bookmarked", dto.Bookmarked);
    command.Parameters.AddWithValue("@trashed", dto.Trashed);

    return command.ExecuteNonQuery() > 0;
  }
  #endregion

  #region Read
  #region Read: Stream
  private async IAsyncEnumerable<NoteDto> GetNotesStream(GetNotesDto dto, [EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    Dictionary<string, object> fields = GetNoteFieldValue(dto);

    if (fields.Count == 0)
      yield break;

    await using SqliteConnection connection = _databaseService.Connection;
    await connection.OpenAsync(cancellationToken);

    string whereClause = string.Join(" AND ", fields.Select(field => $"{field.Key} = @{field.Key}"));
    string limitCluase = dto.Limit > 0 ? "LIMIT @limit" : "";
    string offsetCluase = dto.Offset > 0 ? "OFFSET @offset" : "";
    string orderByClause = GetOrderByClause(dto.SortField, dto.SortDirection);

    // 필요한 필드만(body 제외) 불러오기
    string query = $"""
    SELECT
    id, parent, created, modified, title, preview, background, backdrop, width, height, position_x, position_y, bookmarked, trashed
    FROM Notes
    WHERE {whereClause} {limitCluase} {offsetCluase} {orderByClause}
    """;
    await using SqliteCommand command = new(query, connection);
    foreach (var field in fields)
      command.Parameters.AddWithValue($"@{field.Key}", field.Value);
    if (dto.Limit > 0)
      command.Parameters.AddWithValue("@limit", dto.Limit);
    if (dto.Offset > 0)
      command.Parameters.AddWithValue("@offset", dto.Offset);

    await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
    while (await reader.ReadAsync(cancellationToken))
    {
      //await Task.Delay(100, cancellationToken);
      yield return CreateNoteDtoWithoutBody(reader);
    }
  }

  private async IAsyncEnumerable<NoteDto> SearchNotesStream(IList<GetNoteSearchDto> dtos, [EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    int count = dtos.Count;
    if (count > 0)
    {
      await using SqliteConnection connection = _databaseService.Connection;
      await connection.OpenAsync(cancellationToken);

      List<SqliteCommand> commands = new();

      for (int index = 0; index < count; index += _batchSize)
      {
        int paramsCount = Math.Min(_batchSize, count - index);
        string inClause = string.Join(", ", Enumerable.Range(index, paramsCount).Select((num) => $"@id{num}"));

        string query = $"""
          SELECT
          *
          FROM Notes
          WHERE id IN ({inClause}) AND trashed = @trashed
          """;

        await using SqliteCommand command = new(query, connection);
        command.Parameters.AddWithValue("@trashed", 0);
        for (int paramsIndex = index; paramsIndex < index + paramsCount; paramsIndex++)
          command.Parameters.AddWithValue($"@id{paramsIndex}", dtos[paramsIndex].Id);

        commands.Add(command);
      }

      foreach (var command in commands)
      {
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
          yield return CreateNoteDto(reader);
      }
    }
  }

  public async IAsyncEnumerable<NoteDto> SearchNotesStream(IAsyncEnumerable<GetNoteSearchDto> dtos, [EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    List<SqliteCommand> commands = new();

    List<GetNoteSearchDto> dtoBuffer = new(_batchSize);

    await foreach (var dto in dtos)
    {
      dtoBuffer.Add(dto);
      if (dtoBuffer.Count == _batchSize)
      {
        await foreach (var noteDto in SearchNotesInner(dtoBuffer, _batchSize, cancellationToken))
          yield return noteDto;
        dtoBuffer.Clear();
      }
    }
    if (dtoBuffer.Count > 0)
    {
      await foreach (var noteDto in SearchNotesInner(dtoBuffer, dtoBuffer.Count, cancellationToken))
        yield return noteDto;
    }
  }

  private async IAsyncEnumerable<NoteDto> SearchNotesInner(List<GetNoteSearchDto> dtoBuffer, int batchSize, [EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    await using SqliteConnection connection = _databaseService.Connection;
    await connection.OpenAsync(cancellationToken);

    string inClause = string.Join(", ", Enumerable.Range(0, batchSize).Select((num) => $"@id{num}"));

    string query = $"""
      SELECT
      *
      FROM Notes
      WHERE id IN ({inClause}) AND trashed = @trashed
      """;

    await using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@trashed", 0);
    for (int index = 0; index < batchSize; index++)
      command.Parameters.AddWithValue($"@id{index}", dtoBuffer[index].Id);
    await using SqliteDataReader reader = await command.ExecuteReaderAsync();

    int maxLength = 5000;
    while (await reader.ReadAsync(cancellationToken))
    {
      var dto = dtoBuffer.Find((item) => item.Id == new Guid(GetReaderValue<string>(reader, "id")!));
      string searchPreview = "";
      if (dto is not null)
      {
        string body = GetReaderValue<string>(reader, "body")!;
        var matches = dto.Matches;
        int length = body.Length;
        var firstMatch = matches.OrderBy(pair => pair.Key).First();
        int startOffset = firstMatch.Value.Start.Value;

        if (startOffset <= 0 || length <= maxLength)
          searchPreview = body[0..length];
        else if (startOffset + maxLength >= length)
          searchPreview = "..." + body[(length - maxLength)..length];
        else
          searchPreview = "..." + body[startOffset..(startOffset + maxLength)];
      }
      yield return CreateNoteDto(reader, searchPreview);
    }
  }

  public Task<IAsyncEnumerable<NoteDto>> GetNotesStreamAsync(GetNotesDto dto, CancellationToken cancellationToken = default) => Task.Run(() => GetNotesStream(dto, cancellationToken), cancellationToken);

  public Task<IAsyncEnumerable<NoteDto>> SearchNotesStreamAsync(IList<GetNoteSearchDto> dtos, CancellationToken cancellationToken = default) => Task.Run(() => SearchNotesStream(dtos, cancellationToken), cancellationToken);
  #endregion
  #endregion

  #region Update
  public Task<bool> UpdateNoteAsync(UpdateNoteDto dto)
    => Task.Run(async () =>
    {
      Debug.WriteLine("Update Note");
      Dictionary<string, object> fields = GetNoteUpdateFieldValue(dto);

      if (fields.Count == 0)
        return false;

      string setClause = string.Join(", ", fields.Select(field => $"{field.Key} = @{field.Key}"));
      string query = $"UPDATE Notes SET {setClause} WHERE id = @id";

      using SqliteConnection connection = _databaseService.Connection;
      await connection.OpenAsync();
      using SqliteCommand command = new(query, connection);
      command.Parameters.AddWithValue("@id", dto.Id);
      foreach (var field in fields)
        command.Parameters.AddWithValue($"@{field.Key}", field.Value);

      return await command.ExecuteNonQueryAsync() > 0;
    });
  #endregion

  #region Delete
  public bool DeleteNote(DeleteNoteDto dto)
  {
    using SqliteConnection connection = _databaseService.Connection;
    connection.Open();
    string query = "DELETE FROM Notes WHERE id = @id";
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@id", dto.Id);
    return command.ExecuteNonQuery() > 0;
  }
  #endregion

  #region Helpers & Converters
  private NoteDto CreateNoteDtoWithoutBody(SqliteDataReader reader)
  {
    var id = new Guid(GetReaderValue<string>(reader, "id")!);
    var parent = new Guid(GetReaderValue<string>(reader, "parent")!);
    var created = GetReaderValue<DateTimeOffset>(reader, "created");
    var modified = GetReaderValue<DateTimeOffset>(reader, "modified");
    var title = GetReaderValue<string>(reader, "title")!;
    var preview = GetReaderValue<string>(reader, "preview")!;
    var background = GetReaderValue<string>(reader, "background")!;
    var backdrop = GetReaderValue<int>(reader, "backdrop");
    var width = GetReaderValue<int>(reader, "width");
    var height = GetReaderValue<int>(reader, "height");
    var positionX = GetReaderValue<int>(reader, "position_x");
    var positionY = GetReaderValue<int>(reader, "position_y");
    var bookmarked = GetReaderValue<bool>(reader, "bookmarked");
    var trashed = GetReaderValue<bool>(reader, "trashed");

    NoteDto noteDbDto = new() { Id = id, Parent = parent, Created = created, Modified = modified, Title = title, Body = "", Preview = preview, Background = background, Backdrop = backdrop, Width = width, Height = height, PositionX = positionX, PositionY = positionY, Bookmarked = bookmarked, Trashed = trashed };
    return noteDbDto;
  }

  private NoteDto CreateNoteDto(SqliteDataReader reader, string? searchPreview = null)
  {
    var id = new Guid(GetReaderValue<string>(reader, "id")!);
    var parent = new Guid(GetReaderValue<string>(reader, "parent")!);
    var created = GetReaderValue<DateTimeOffset>(reader, "created");
    var modified = GetReaderValue<DateTimeOffset>(reader, "modified");
    var title = GetReaderValue<string>(reader, "title")!;
    var body = GetReaderValue<string>(reader, "body")!;
    var preview = searchPreview is null ? GetReaderValue<string>(reader, "preview")! : searchPreview;
    var background = GetReaderValue<string>(reader, "background")!;
    var backdrop = GetReaderValue<int>(reader, "backdrop");
    var width = GetReaderValue<int>(reader, "width");
    var height = GetReaderValue<int>(reader, "height");
    var positionX = GetReaderValue<int>(reader, "position_x");
    var positionY = GetReaderValue<int>(reader, "position_y");
    var bookmarked = GetReaderValue<bool>(reader, "bookmarked");
    var trashed = GetReaderValue<bool>(reader, "trashed");

    NoteDto noteDbDto = new() { Id = id, Parent = parent, Created = created, Modified = modified, Title = title, Body = body, Preview = preview, Background = background, Backdrop = backdrop, Width = width, Height = height, PositionX = positionX, PositionY = positionY, Bookmarked = bookmarked, Trashed = trashed };
    return noteDbDto;
  }

  private HashSet<string> GetNoteCreateFieldValue(NoteFields noteFields)
  {
    HashSet<string> fields = new();

    if (noteFields == NoteFields.None)
      return fields;
    if (noteFields.HasFlag(NoteFields.Id))
      fields.Add("id");
    if (noteFields.HasFlag(NoteFields.Parent))
      fields.Add("parent");
    if (noteFields.HasFlag(NoteFields.Created))
      fields.Add("created");
    if (noteFields.HasFlag(NoteFields.Modified))
      fields.Add("modified");
    if (noteFields.HasFlag(NoteFields.Title))
      fields.Add("title");
    if (noteFields.HasFlag(NoteFields.Body))
      fields.Add("body");
    if (noteFields.HasFlag(NoteFields.Preview))
      fields.Add("preview");
    if (noteFields.HasFlag(NoteFields.Background))
      fields.Add("background");
    if (noteFields.HasFlag(NoteFields.Backdrop))
      fields.Add("backdrop");
    if (noteFields.HasFlag(NoteFields.Width))
      fields.Add("width");
    if (noteFields.HasFlag(NoteFields.Height))
      fields.Add("height");
    if (noteFields.HasFlag(NoteFields.PositionX))
      fields.Add("position_x");
    if (noteFields.HasFlag(NoteFields.PositionY))
      fields.Add("position_y");
    if (noteFields.HasFlag(NoteFields.Bookmarked))
      fields.Add("bookmarked");
    if (noteFields.HasFlag(NoteFields.Trashed))
      fields.Add("trashed");
    return fields;
  }

  private Dictionary<string, object> GetNoteUpdateFieldValue(UpdateNoteDto dto)
  {
    Dictionary<string, object> fields = new();
    var updateFields = dto.UpdateFields;

    if (updateFields == NoteUpdateFields.None)
      return fields;
    if (updateFields.HasFlag(NoteUpdateFields.Parent) && dto.Parent is not null)
      fields.Add("parent", dto.Parent);
    if (updateFields.HasFlag(NoteUpdateFields.Modified) && dto.Modified is not null)
      fields.Add("modified", dto.Modified);
    if (updateFields.HasFlag(NoteUpdateFields.Title) && dto.Title is not null)
      fields.Add("title", dto.Title);
    if (updateFields.HasFlag(NoteUpdateFields.Body) && dto.Body is not null)
      fields.Add("body", dto.Body);
    if (updateFields.HasFlag(NoteUpdateFields.Preview) && dto.Preview is not null)
      fields.Add("preview", dto.Preview);
    if (updateFields.HasFlag(NoteUpdateFields.Background) && dto.Background is not null)
      fields.Add("background", dto.Background);
    if (updateFields.HasFlag(NoteUpdateFields.Backdrop) && dto.Backdrop is not null)
      fields.Add("backdrop", dto.Backdrop);
    if (updateFields.HasFlag(NoteUpdateFields.Width) && dto.Width is not null)
      fields.Add("width", dto.Width);
    if (updateFields.HasFlag(NoteUpdateFields.Height) && dto.Height is not null)
      fields.Add("height", dto.Height);
    if (updateFields.HasFlag(NoteUpdateFields.PositionX) && dto.PositionX is not null)
      fields.Add("position_x", dto.PositionX);
    if (updateFields.HasFlag(NoteUpdateFields.PositionY) && dto.PositionY is not null)
      fields.Add("position_y", dto.PositionY);
    if (updateFields.HasFlag(NoteUpdateFields.Bookmarked) && dto.Bookmarked is not null)
      fields.Add("bookmarked", dto.Bookmarked);
    if (updateFields.HasFlag(NoteUpdateFields.Trashed) && dto.Trashed is not null)
      fields.Add("trashed", dto.Trashed);
    return fields;
  }

  private Dictionary<string, object> GetNoteFieldValue(GetNotesDto dto)
  {
    Dictionary<string, object> fields = new();
    var getFields = dto.GetFields;
    if (getFields == NoteGetFields.None)
      return fields;

    if (getFields.HasFlag(NoteGetFields.Parent) && dto.Parent is not null)
      fields.Add("parent", dto.Parent);
    if (getFields.HasFlag(NoteGetFields.Bookmarked) && dto.Bookmarked is not null)
      fields.Add("bookmarked", dto.Bookmarked);
    if (getFields.HasFlag(NoteGetFields.Trashed) && dto.Trashed is not null)
      fields.Add("trashed", dto.Trashed);
    return fields;
  }

  private string GetOrderByClause(NoteSortField? field, SortDirection? direction)
  {
    if (field is null || direction is null)
      return "";
    string orderByClause = "ORDER BY ";
    orderByClause += field switch
    {
      NoteSortField.Modified => "modified ",
      NoteSortField.Title => "title ",
      NoteSortField.Created or _ => "created "
    };
    orderByClause += direction switch
    {
      SortDirection.Descending => "DESC",
      SortDirection.Ascending or _ => "ASC"
    };
    return orderByClause;
  }
  #endregion
}
