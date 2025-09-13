using System.Runtime.CompilerServices;
using System.Threading;

using Microsoft.Data.Sqlite;

using MyNotes.Common.Collections;
using MyNotes.Core.Dto;
using MyNotes.Core.Model;
using MyNotes.Core.Service;

using Col = MyNotes.Core.Shared.DatabaseSettings.Column;
using Param = MyNotes.Core.Shared.DatabaseSettings.Parameter;
using Tbl = MyNotes.Core.Shared.DatabaseSettings.Table;

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
    string query = $"""
      INSERT OR IGNORE INTO {Tbl.Notes}
      (
      {Col.Id}, {Col.Parent}, {Col.Created}, {Col.Modified}, {Col.Title},
      {Col.Body}, {Col.Preview}, {Col.Background}, {Col.Backdrop}, {Col.Width},
      {Col.Height}, {Col.PositionX}, {Col.PositionY}, {Col.Bookmarked}, {Col.Trashed}
      )
      VALUES
      (
      {Param.Id}, {Param.Parent}, {Param.Created}, {Param.Modified}, {Param.Title},
      {Param.Body}, {Param.Preview}, {Param.Background}, {Param.Backdrop}, {Param.Width},
      {Param.Height}, {Param.PositionX}, {Param.PositionY}, {Param.Bookmarked}, {Param.Trashed}
      )
      """;

    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue(Param.Id, dto.Id);
    command.Parameters.AddWithValue(Param.Parent, dto.Parent);
    command.Parameters.AddWithValue(Param.Created, dto.Created);
    command.Parameters.AddWithValue(Param.Modified, dto.Modified);
    command.Parameters.AddWithValue(Param.Title, dto.Title);
    command.Parameters.AddWithValue(Param.Body, dto.Body);
    command.Parameters.AddWithValue(Param.Preview, dto.Preview);
    command.Parameters.AddWithValue(Param.Background, dto.Background);
    command.Parameters.AddWithValue(Param.Backdrop, dto.Backdrop);
    command.Parameters.AddWithValue(Param.Width, dto.Width);
    command.Parameters.AddWithValue(Param.Height, dto.Height);
    command.Parameters.AddWithValue(Param.PositionX, dto.PositionX);
    command.Parameters.AddWithValue(Param.PositionY, dto.PositionY);
    command.Parameters.AddWithValue(Param.Bookmarked, dto.Bookmarked);
    command.Parameters.AddWithValue(Param.Trashed, dto.Trashed);

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
    {Col.Id}, {Col.Parent}, {Col.Created}, {Col.Modified}, {Col.Title},
    {Col.Preview}, {Col.Background}, {Col.Backdrop}, {Col.Width},
    {Col.Height}, {Col.PositionX}, {Col.PositionY}, {Col.Bookmarked}, {Col.Trashed}
    FROM {Tbl.Notes}
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
        string inClause = string.Join(", ", Enumerable.Range(index, paramsCount).Select((num) => $"{Param.Id}{num}"));

        string query = $"""
          SELECT
          *
          FROM {Tbl.Notes}
          WHERE {Col.Id} IN ({inClause}) AND {Col.Trashed} = {Param.Trashed}
          """;

        await using SqliteCommand command = new(query, connection);
        command.Parameters.AddWithValue(Param.Trashed, 0);
        for (int paramsIndex = index; paramsIndex < index + paramsCount; paramsIndex++)
          command.Parameters.AddWithValue($"{Param.Id}{paramsIndex}", dtos[paramsIndex].Id);

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

    string inClause = string.Join(", ", Enumerable.Range(0, batchSize).Select((num) => $"{Param.Id}{num}"));

    string query = $"""
      SELECT
      *
      FROM {Tbl.Notes}
      WHERE {Col.Id} IN ({inClause}) AND {Col.Trashed} = {Param.Trashed}
      """;

    await using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue(Param.Trashed, 0);
    for (int index = 0; index < batchSize; index++)
      command.Parameters.AddWithValue($"{Param.Id}{index}", dtoBuffer[index].Id);
    await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

    int maxLength = 5000;
    while (await reader.ReadAsync(cancellationToken))
    {
      var dto = dtoBuffer.Find((item) => item.Id == new Guid(GetReaderValue<string>(reader, Col.Id)!));
      string searchPreview = "";
      if (dto is not null)
      {
        string body = GetReaderValue<string>(reader, Col.Body)!;
        var matches = dto.Matches;
        int length = body.Length;
        var firstMatch = matches.OrderBy(pair => pair.Key).First();
        int startOffset = firstMatch.Value.Start.Value;

        if (startOffset <= 0 || length <= maxLength)
          searchPreview = body[0..length];
        else if (startOffset + maxLength >= length)
          searchPreview = "..." + body[(length - maxLength)..length];
        else
          searchPreview = "..." + body[Math.Max(0, startOffset - 20)..(startOffset + maxLength)];
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
      string query = $"UPDATE {Tbl.Notes} SET {setClause} WHERE {Col.Id} = {Param.Id}";

      using SqliteConnection connection = _databaseService.Connection;
      await connection.OpenAsync();
      using SqliteCommand command = new(query, connection);
      command.Parameters.AddWithValue(Param.Id, dto.Id);
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
    string query = $"DELETE FROM {Tbl.Notes} WHERE {Col.Id} = {Param.Id}";
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue(Param.Id, dto.Id);
    return command.ExecuteNonQuery() > 0;
  }
  #endregion

  #region Helpers & Converters
  private NoteDto CreateNoteDtoWithoutBody(SqliteDataReader reader)
  {
    var id = new Guid(GetReaderValue<string>(reader, Col.Id)!);
    var parent = new Guid(GetReaderValue<string>(reader, Col.Parent)!);
    var created = GetReaderValue<DateTimeOffset>(reader, Col.Created);
    var modified = GetReaderValue<DateTimeOffset>(reader, Col.Modified);
    var title = GetReaderValue<string>(reader, Col.Title)!;
    var preview = GetReaderValue<string>(reader, Col.Preview)!;
    var background = GetReaderValue<string>(reader, Col.Background)!;
    var backdrop = GetReaderValue<int>(reader, Col.Backdrop);
    var width = GetReaderValue<int>(reader, Col.Width);
    var height = GetReaderValue<int>(reader, Col.Height);
    var positionX = GetReaderValue<int>(reader, Col.PositionX);
    var positionY = GetReaderValue<int>(reader, Col.PositionY);
    var bookmarked = GetReaderValue<bool>(reader, Col.Bookmarked);
    var trashed = GetReaderValue<bool>(reader, Col.Trashed);

    NoteDto noteDbDto = new()
    {
      Id = id,
      Parent = parent,
      Created = created,
      Modified = modified,
      Title = title,
      Body = "",
      Preview = preview,
      Background = background,
      Backdrop = backdrop,
      Width = width,
      Height = height,
      PositionX = positionX,
      PositionY = positionY,
      Bookmarked = bookmarked,
      Trashed = trashed
    };
    return noteDbDto;
  }

  private NoteDto CreateNoteDto(SqliteDataReader reader, string? searchPreview = null)
  {
    var id = new Guid(GetReaderValue<string>(reader, Col.Id)!);
    var parent = new Guid(GetReaderValue<string>(reader, Col.Parent)!);
    var created = GetReaderValue<DateTimeOffset>(reader, Col.Created);
    var modified = GetReaderValue<DateTimeOffset>(reader, Col.Modified);
    var title = GetReaderValue<string>(reader, Col.Title)!;
    var body = GetReaderValue<string>(reader, Col.Body)!;
    var preview = searchPreview is null ? GetReaderValue<string>(reader, Col.Preview)! : searchPreview;
    var background = GetReaderValue<string>(reader, Col.Background)!;
    var backdrop = GetReaderValue<int>(reader, Col.Backdrop);
    var width = GetReaderValue<int>(reader, Col.Width);
    var height = GetReaderValue<int>(reader, Col.Height);
    var positionX = GetReaderValue<int>(reader, Col.PositionX);
    var positionY = GetReaderValue<int>(reader, Col.PositionY);
    var bookmarked = GetReaderValue<bool>(reader, Col.Bookmarked);
    var trashed = GetReaderValue<bool>(reader, Col.Trashed);

    NoteDto noteDbDto = new()
    {
      Id = id,
      Parent = parent,
      Created = created,
      Modified = modified,
      Title = title,
      Body = body,
      Preview = preview,
      Background = background,
      Backdrop = backdrop,
      Width = width,
      Height = height,
      PositionX = positionX,
      PositionY = positionY,
      Bookmarked = bookmarked,
      Trashed = trashed
    };
    return noteDbDto;
  }

  private Dictionary<string, object> GetNoteUpdateFieldValue(UpdateNoteDto dto)
  {
    Dictionary<string, object> fields = new();
    var updateFields = dto.UpdateFields;

    if (updateFields == NoteUpdateFields.None)
      return fields;
    if (updateFields.HasFlag(NoteUpdateFields.Parent) && dto.Parent is not null)
      fields.Add(Col.Parent, dto.Parent);
    if (updateFields.HasFlag(NoteUpdateFields.Modified) && dto.Modified is not null)
      fields.Add(Col.Modified, dto.Modified);
    if (updateFields.HasFlag(NoteUpdateFields.Title) && dto.Title is not null)
      fields.Add(Col.Title, dto.Title);
    if (updateFields.HasFlag(NoteUpdateFields.Body) && dto.Body is not null)
      fields.Add(Col.Body, dto.Body);
    if (updateFields.HasFlag(NoteUpdateFields.Preview) && dto.Preview is not null)
      fields.Add(Col.Preview, dto.Preview);
    if (updateFields.HasFlag(NoteUpdateFields.Background) && dto.Background is not null)
      fields.Add(Col.Background, dto.Background);
    if (updateFields.HasFlag(NoteUpdateFields.Backdrop) && dto.Backdrop is not null)
      fields.Add(Col.Backdrop, dto.Backdrop);
    if (updateFields.HasFlag(NoteUpdateFields.Width) && dto.Width is not null)
      fields.Add(Col.Width, dto.Width);
    if (updateFields.HasFlag(NoteUpdateFields.Height) && dto.Height is not null)
      fields.Add(Col.Height, dto.Height);
    if (updateFields.HasFlag(NoteUpdateFields.PositionX) && dto.PositionX is not null)
      fields.Add(Col.PositionX, dto.PositionX);
    if (updateFields.HasFlag(NoteUpdateFields.PositionY) && dto.PositionY is not null)
      fields.Add(Col.PositionY, dto.PositionY);
    if (updateFields.HasFlag(NoteUpdateFields.Bookmarked) && dto.Bookmarked is not null)
      fields.Add(Col.Bookmarked, dto.Bookmarked);
    if (updateFields.HasFlag(NoteUpdateFields.Trashed) && dto.Trashed is not null)
      fields.Add(Col.Trashed, dto.Trashed);
    return fields;
  }

  private Dictionary<string, object> GetNoteFieldValue(GetNotesDto dto)
  {
    Dictionary<string, object> fields = new();
    var getFields = dto.GetFields;
    if (getFields == NoteGetFields.None)
      return fields;

    if (getFields.HasFlag(NoteGetFields.Parent) && dto.Parent is not null)
      fields.Add(Col.Parent, dto.Parent);
    if (getFields.HasFlag(NoteGetFields.Bookmarked) && dto.Bookmarked is not null)
      fields.Add(Col.Bookmarked, dto.Bookmarked);
    if (getFields.HasFlag(NoteGetFields.Trashed) && dto.Trashed is not null)
      fields.Add(Col.Trashed, dto.Trashed);
    return fields;
  }

  private string GetOrderByClause(NoteSortField? field, SortDirection? direction)
  {
    if (field is null || direction is null)
      return "";
    string orderByClause = "ORDER BY ";
    orderByClause += field switch
    {
      NoteSortField.Modified => $"{Col.Modified} ",
      NoteSortField.Title => $"{Col.Title} ",
      NoteSortField.Created or _ => $"{Col.Created} "
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
