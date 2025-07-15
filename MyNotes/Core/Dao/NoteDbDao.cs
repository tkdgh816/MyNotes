using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;

using Microsoft.Data.Sqlite;

using MyNotes.Core.Dto;
using MyNotes.Core.Service;

namespace MyNotes.Core.Dao;
internal class NoteDbDao(DatabaseService databaseService) : DbDaoBase
{
  private readonly DatabaseService _databaseService = databaseService;
  

  public bool AddNote(NoteDto dto)
  {
    using SqliteConnection connection = _databaseService.Connection;
    connection.Open();
    string query = """
      INSERT OR IGNORE INTO Notes
      (id, parent, created, modified, title, body, background, backdrop, width, height, position_x, position_y, bookmarked, trashed)
      VALUES
      (@id, @parent, @created, @modified, @title, @body, @background, @backdrop, @width, @height, @position_x, @position_y, @bookmarked, @trashed)
      """;
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@id", dto.Id);
    command.Parameters.AddWithValue("@parent", dto.Parent);
    command.Parameters.AddWithValue("@created", dto.Created);
    command.Parameters.AddWithValue("@modified", dto.Modified);
    command.Parameters.AddWithValue("@title", dto.Title);
    command.Parameters.AddWithValue("@body", dto.Body);
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

  public bool DeleteNote(DeleteNoteDto dto)
  {
    using SqliteConnection connection = _databaseService.Connection;
    connection.Open();
    string query = "DELETE FROM Notes WHERE id = @id";
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@id", dto.Id);
    return command.ExecuteNonQuery() > 0;
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

  public bool UpdateNote(UpdateNoteDto dto)
  {
    Dictionary<string, object> fields = GetNoteUpdateFieldValue(dto);

    if (fields.Count == 0)
      return false;

    string setClause = string.Join(", ", fields.Select(field => $"{field.Key} = @{field.Key}"));
    string query = $"UPDATE Notes SET {setClause} WHERE id = @id";

    using SqliteConnection connection = _databaseService.Connection;
    connection.Open();
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@id", dto.Id);
    foreach (var field in fields)
      command.Parameters.AddWithValue($"@{field.Key}", field.Value);

    return command.ExecuteNonQuery() > 0;
  }

  

  public async Task<IEnumerable<NoteDto>> GetNotes(GetBoardNotesDto dto)
  {
    List<NoteDto> noteDtos = new();
    await using SqliteConnection connection = _databaseService.Connection;
    await connection.OpenAsync();

    string query = "SELECT * FROM Notes WHERE parent = @parent AND trashed = 0";
    await using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@parent", dto.Id);

    await using SqliteDataReader reader = command.ExecuteReader();
    while (await reader.ReadAsync())
      noteDtos.Add(CreateNoteDto(reader));

    return noteDtos;
  }

  public async Task<IEnumerable<NoteDto>> GetBookmarkedNotes()
  {
    List<NoteDto> noteDtos = new();
    await using SqliteConnection connection = _databaseService.Connection;
    await connection.OpenAsync();

    string query = "SELECT * FROM Notes WHERE bookmarked = 1 AND trashed = 0";
    await using SqliteCommand command = new(query, connection);

    await using SqliteDataReader reader = command.ExecuteReader();
    while (await reader.ReadAsync())
      noteDtos.Add(CreateNoteDto(reader));

    return noteDtos;
  }

  public async Task<IEnumerable<NoteDto>> GetTrashedNotes()
  {
    List<NoteDto> noteDtos = new();
    await using SqliteConnection connection = _databaseService.Connection;
    await connection.OpenAsync();

    string query = "SELECT * FROM Notes WHERE trashed = 1";
    await using SqliteCommand command = new(query, connection);

    await using SqliteDataReader reader = command.ExecuteReader();
    while (await reader.ReadAsync())
      noteDtos.Add(CreateNoteDto(reader));

    return noteDtos;
  }

  public async Task<IEnumerable<NoteDto>> SearchNotes(IList<GetNoteDto> dtos)
  {
    List<NoteDto> noteDtos = new();

    int count = dtos.Count;
    if (count > 0)
    {
      await using SqliteConnection connection = _databaseService.Connection;
      await connection.OpenAsync();

      List<SqliteCommand> commands = new();

      int batchSize = 10;
      for (int index = 0; index < count; index += batchSize)
      {
        int paramsCount = Math.Min(batchSize, count - index);
        string query = $"SELECT * FROM Notes WHERE id IN ({string.Join(", ", Enumerable.Range(index, paramsCount).Select((num) => $"@id{num}"))})";

        await using SqliteCommand command = new(query, connection);
        for (int paramsIndex = index; paramsIndex < index + paramsCount; paramsIndex++)
          command.Parameters.AddWithValue($"@id{paramsIndex}", dtos[paramsIndex].Id);

        commands.Add(command);
      }

      foreach (var command in commands)
      {
        await using SqliteDataReader reader = command.ExecuteReader();
        while (await reader.ReadAsync())
          noteDtos.Add(CreateNoteDto(reader));
      }
    }

    return noteDtos;
  }

  private NoteDto CreateNoteDto(SqliteDataReader reader)
  {
    var id = new Guid(GetReaderValue<string>(reader, "id")!);
    var parent = new Guid(GetReaderValue<string>(reader, "parent")!);
    var created = GetReaderValue<DateTimeOffset>(reader, "created");
    var modified = GetReaderValue<DateTimeOffset>(reader, "modified");
    var title = GetReaderValue<string>(reader, "title")!;
    var body = GetReaderValue<string>(reader, "body")!;
    var background = GetReaderValue<string>(reader, "background")!;
    var backdrop = GetReaderValue<int>(reader, "backdrop");
    var width = GetReaderValue<int>(reader, "width");
    var height = GetReaderValue<int>(reader, "height");
    var positionX = GetReaderValue<int>(reader, "position_x");
    var positionY = GetReaderValue<int>(reader, "position_y");
    var bookmarked = GetReaderValue<bool>(reader, "bookmarked");
    var trashed = GetReaderValue<bool>(reader, "trashed");

    NoteDto noteDbDto = new() { Id = id, Parent = parent, Created = created, Modified = modified, Title = title, Body = body, Background = background, Backdrop = backdrop, Width = width, Height = height, PositionX = positionX, PositionY = positionY, Bookmarked = bookmarked, Trashed = trashed };
    return noteDbDto;
  }
}
