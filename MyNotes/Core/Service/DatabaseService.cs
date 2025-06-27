using Microsoft.Data.Sqlite;

using MyNotes.Core.Dto;

namespace MyNotes.Core.Service;

internal class DatabaseService
{
  private readonly StorageFolder _localFolder = ApplicationData.Current.LocalFolder;
  private readonly string _connectionString;

  public DatabaseService()
  {
    var databaseFolder = _localFolder.CreateFolderAsync("data", CreationCollisionOption.OpenIfExists).GetAwaiter().GetResult();
    _connectionString = new SqliteConnectionStringBuilder()
    {
      DataSource = Path.Combine(databaseFolder.Path, "data.sqlite"),
      ForeignKeys = true
    }.ToString();

    CreateTable();
  }

  #region Create Table
  private void CreateTable()
  {
    using SqliteConnection connection = new(_connectionString);
    connection.Open();

    string createBoardsQuery =
@"CREATE TABLE IF NOT EXISTS Boards
(
id              TEXT PRIMARY KEY NOT NULL,
grouped         INTEGER NOT NULL DEFAULT 0,
parent          TEXT NULL NOT NULL,
previous        TEXT NULL,
name            TEXT NOT NULL DEFAULT '',
icon_type       INTEGER NOT NULL,
icon_value      INTEGER NOT NULL
)";

    string createNotesQuery =
  @"CREATE TABLE IF NOT EXISTS Notes
(
id              TEXT PRIMARY KEY NOT NULL,
parent          TEXT NOT NULL,
created         TEXT NOT NULL,
modified        TEXT NOT NULL,
title           TEXT NOT NULL DEFAULT '',
body            TEXT NOT NULL DEFAULT '',
background      TEXT NOT NULL,
backdrop        INTEGER NOT NULL,
width           INTEGER NOT NULL,
height          INTEGER NOT NULL,
position_x      INTEGER NOT NULL,
position_y      INTEGER NOT NULL,
bookmarked      INTEGER NOT NULL DEFAULT 0,
trashed         INTEGER NOT NULL DEFAULT 0
)";

    string createTagsQuery =
  @"CREATE TABLE IF NOT EXISTS Tags
(
id              TEXT PRIMARY KEY NOT NULL,
text            TEXT NOT NULL UNIQUE,
color           INTEGER NOT NULL
)";

    string createNotesTagsQuery =
  @"CREATE TABLE IF NOT EXISTS NotesTags
(
note_id         TEXT NOT NULL,
tag_id          TEXT NOT NULL,
PRIMARY KEY (note_id, tag_id),
FOREIGN KEY (note_id) REFERENCES Notes(id) ON DELETE CASCADE ON UPDATE CASCADE,
FOREIGN KEY (tag_id) REFERENCES Tags(id) ON DELETE CASCADE ON UPDATE CASCADE
)";

    using (SqliteCommand command = new(createBoardsQuery, connection))
      command.ExecuteNonQuery();
    using (SqliteCommand command = new(createNotesQuery, connection))
      command.ExecuteNonQuery();
    using (SqliteCommand command = new(createTagsQuery, connection))
      command.ExecuteNonQuery();
    using (SqliteCommand command = new(createNotesTagsQuery, connection))
      command.ExecuteNonQuery();

  }
  #endregion

  #region Boards
  public void AddBoard(InsertBoardDbDto dto)
  {
    using SqliteConnection connection = new(_connectionString);
    connection.Open();

    object previous = dto.Previous is null ? DBNull.Value : dto.Previous;

    if (dto.Next is not null)
    {
      string updateNextQuery = @"UPDATE Boards SET previous = @previous WHERE id = @id";
      using SqliteCommand updateNextCommand = new(updateNextQuery, connection);
      updateNextCommand.Parameters.AddWithValue("@id", dto.Next);
      updateNextCommand.Parameters.AddWithValue("@previous", dto.Id);
      updateNextCommand.ExecuteNonQuery();
    }

    string insertBoardQuery = @"INSERT OR IGNORE INTO Boards
(id, grouped, parent, previous, name, icon_type, icon_value)
VALUES
(@id, @grouped, @parent, @previous, @name, @icon_type, @icon_value)
";
    using SqliteCommand insertBoardCommand = new(insertBoardQuery, connection);
    insertBoardCommand.Parameters.AddWithValue("@id", dto.Id);
    insertBoardCommand.Parameters.AddWithValue("@grouped", dto.Grouped);
    insertBoardCommand.Parameters.AddWithValue("@parent", dto.Parent);
    insertBoardCommand.Parameters.AddWithValue("@previous", previous);
    insertBoardCommand.Parameters.AddWithValue("@name", dto.Name);
    insertBoardCommand.Parameters.AddWithValue("@icon_type", dto.IconType);
    insertBoardCommand.Parameters.AddWithValue("@icon_value", dto.IconValue);
    insertBoardCommand.ExecuteNonQuery();
  }

  public async Task<IEnumerable<BoardDbDto>> GetBoards()
  {
    List<BoardDbDto> boards = new();
    await using SqliteConnection connection = new(_connectionString);
    await connection.OpenAsync();

    string query = @"SELECT * FROM Boards";
    await using SqliteCommand command = new(query, connection);
    await using SqliteDataReader reader = command.ExecuteReader();
    while (await reader.ReadAsync())
    {
      Guid id = GetReaderValue<Guid>(reader, "id")!;
      bool grouped = GetReaderValue<bool>(reader, "grouped")!;
      Guid parent = GetReaderValue<Guid>(reader, "parent")!;
      Guid previous = GetReaderValue<Guid>(reader, "previous")!;
      string name = GetReaderValue<string>(reader, "name")!;
      int iconType = GetReaderValue<int>(reader, "icon_type");
      int iconValue = GetReaderValue<int>(reader, "icon_value");
      boards.Add(new BoardDbDto() { Id = id, Grouped = grouped, Parent = parent, Previous = previous, Name = name, IconType = iconType, IconValue = iconValue });
    }

    return boards;
  }

  private Dictionary<string, object> GetBoardUpdateFieldValue(UpdateBoardDbDto dto)
  {
    Dictionary<string, object> fields = new();
    var updateFields = dto.UpdateFields;
    if (updateFields == BoardUpdateFields.None)
      return fields;

    if (updateFields.HasFlag(BoardUpdateFields.Parent) && dto.Parent is not null)
      fields.Add("parent", dto.Parent);
    if (updateFields.HasFlag(BoardUpdateFields.Previous))
      fields.Add("previous", dto.Previous is null ? DBNull.Value : dto.Previous);
    if (updateFields.HasFlag(BoardUpdateFields.Name) && dto.Name is not null)
      fields.Add("name", dto.Name);
    if (updateFields.HasFlag(BoardUpdateFields.IconType) && dto.IconType is not null)
      fields.Add("icon_type", dto.IconType);
    if (updateFields.HasFlag(BoardUpdateFields.IconValue) && dto.IconValue is not null)
      fields.Add("icon_value", dto.IconValue);
    return fields;
  }

  public void UpdateBoard(UpdateBoardDbDto dto)
  {
    Dictionary<string, object> fields = GetBoardUpdateFieldValue(dto);

    if (fields.Count == 0)
      return;

    string setClause = string.Join(", ", fields.Select(field => $"{field.Key} = @{field.Key}"));
    string query = @$"UPDATE Boards SET {setClause} WHERE id = @id";

    using SqliteConnection connection = new(_connectionString);
    connection.Open();
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@id", dto.Id);
    foreach (var field in fields)
      command.Parameters.AddWithValue($"@{field.Key}", field.Value);
    command.ExecuteNonQuery();
  }

  public void DeleteBoard(DeleteBoardDbDto dto)
  {
    using SqliteConnection connection = new(_connectionString);
    connection.Open();

    object? previous = null;
    object? next = null;

    string getPreviousQuery = @"SELECT previous from boards WHERE id = @id";
    using SqliteCommand getPreviousCommand = new(getPreviousQuery, connection);
    getPreviousCommand.Parameters.AddWithValue("@id", dto.Id);
    using SqliteDataReader getPreviousReader = getPreviousCommand.ExecuteReader();
    if (getPreviousReader.Read())
    {
      Guid dbPrevious = GetReaderValue<Guid>(getPreviousReader, "previous");
      previous = dbPrevious == Guid.Empty ? null : dbPrevious;
    }

    string getNextQuery = @"SELECT id from boards WHERE previous = @previous";
    using SqliteCommand getNextCommand = new(getNextQuery, connection);
    getNextCommand.Parameters.AddWithValue("@previous", dto.Id);
    using SqliteDataReader getNextReader = getNextCommand.ExecuteReader();
    if (getNextReader.Read())
      next = GetReaderValue<Guid>(getNextReader, "id");

    string deleteBoardQuery = @"DELETE from Boards WHERE id = @id";
    using SqliteCommand deleteBoardCommand = new(deleteBoardQuery, connection);
    deleteBoardCommand.Parameters.AddWithValue("@id", dto.Id);
    if (deleteBoardCommand.ExecuteNonQuery() > 0)
    {
      previous = previous is null ? DBNull.Value : previous;
      if (next is not null)
      {
        string updateNextQuery = @"UPDATE Boards SET previous = @previous WHERE id = @id";
        using SqliteCommand updateNextCommand = new(updateNextQuery, connection);
        updateNextCommand.Parameters.AddWithValue("@id", next);
        updateNextCommand.Parameters.AddWithValue("@previous", previous);
        updateNextCommand.ExecuteNonQuery();
      }

      string removeNotesQuery = @"UPDATE Notes SET trashed = 1 WHERE parent = @parent";
      using SqliteCommand removeNotesCommand = new(removeNotesQuery, connection);
      removeNotesCommand.Parameters.AddWithValue("@parent", dto.Id);
      removeNotesCommand.ExecuteNonQuery();
    }
  }
  #endregion

  #region Notes
  public bool AddNote(NoteDbDto dto)
  {
    using SqliteConnection connection = new(_connectionString);
    connection.Open();
    string query = @"INSERT OR IGNORE INTO Notes
(id, parent, created, modified, title, body, background, backdrop, width, height, position_x, position_y, bookmarked, trashed)
VALUES
(@id, @parent, @created, @modified, @title, @body, @background, @backdrop, @width, @height, @position_x, @position_y, @bookmarked, @trashed)
";
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

  public bool DeleteNote(DeleteNoteDbDto dto)
  {
    using SqliteConnection connection = new(_connectionString);
    connection.Open();
    string query = @"DELETE FROM Notes WHERE id = @id";
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@id", dto.Id);
    return command.ExecuteNonQuery() > 0;
  }

  private Dictionary<string, object> GetNoteUpdateFieldValue(UpdateNoteDbDto dto)
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

  public bool UpdateNote(UpdateNoteDbDto dto)
  {
    Dictionary<string, object> fields = GetNoteUpdateFieldValue(dto);

    if (fields.Count == 0)
      return false;

    string setClause = string.Join(", ", fields.Select(field => $"{field.Key} = @{field.Key}"));
    string query = @$"UPDATE Notes SET {setClause} WHERE id = @id";

    using SqliteConnection connection = new(_connectionString);
    connection.Open();
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@id", dto.Id);
    foreach (var field in fields)
    {
      command.Parameters.AddWithValue($"@{field.Key}", field.Value);
      //Debug.WriteLine($"DBUpdate: [ {field.Key}, {field.Value} ]");
    }
    return command.ExecuteNonQuery() > 0;
  }

  public async Task<IEnumerable<NoteDbDto>> GetNotes(GetBoardNotesDbDto dto)
  {
    List<NoteDbDto> notes = new();
    await using SqliteConnection connection = new(_connectionString);
    await connection.OpenAsync();

    string query = "SELECT * FROM Notes WHERE parent = @parent AND trashed = 0";
    await using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@parent", dto.Id);

    await using SqliteDataReader reader = command.ExecuteReader();
    while (await reader.ReadAsync())
      notes.Add(CreateNoteDto(reader));

    return notes;
  }

  public async Task<IEnumerable<NoteDbDto>> GetBookmarkedNotes()
  {
    List<NoteDbDto> notes = new();
    await using SqliteConnection connection = new(_connectionString);
    await connection.OpenAsync();

    string query = "SELECT * FROM Notes WHERE bookmarked = 1 AND trashed = 0";
    await using SqliteCommand command = new(query, connection);

    await using SqliteDataReader reader = command.ExecuteReader();
    while (await reader.ReadAsync())
      notes.Add(CreateNoteDto(reader));

    return notes;
  }

  public async Task<IEnumerable<NoteDbDto>> GetTrashedNotes()
  {
    List<NoteDbDto> notes = new();
    await using SqliteConnection connection = new(_connectionString);
    await connection.OpenAsync();

    string query = "SELECT * FROM Notes WHERE trashed = 1";
    await using SqliteCommand command = new(query, connection);

    await using SqliteDataReader reader = command.ExecuteReader();
    while (await reader.ReadAsync())
      notes.Add(CreateNoteDto(reader));

    return notes;
  }

  private NoteDbDto CreateNoteDto(SqliteDataReader reader)
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

    NoteDbDto noteDbDto = new() { Id = id, Parent = parent, Created = created, Modified = modified, Title = title, Body = body, Background = background, Backdrop = backdrop, Width = width, Height = height, PositionX = positionX, PositionY = positionY, Bookmarked = bookmarked, Trashed = trashed };
    return noteDbDto;
  }
  #endregion

  #region Tags
  public async Task<IEnumerable<TagDbDto>> GetTags(GetNoteTagsDbDto dto)
  {
    List<TagDbDto> tags = new();
    await using SqliteConnection connection = new(_connectionString);
    await connection.OpenAsync();

    string query =
@"SELECT * FROM Tags
INNER JOIN NotesTags ON Tags.id = NotesTags.tag_id
WHERE NotesTags.note_id = @note_id";
    await using SqliteCommand command = new(query, connection);

    command.Parameters.AddWithValue("@note_id", dto.NoteId);

    await using SqliteDataReader reader = command.ExecuteReader();
    while (await reader.ReadAsync())
    {
      var id = GetReaderValue<Guid>(reader, "id");
      var tag = GetReaderValue<string>(reader, "text")!;
      var color = GetReaderValue<int>(reader, "color");
      tags.Add(new TagDbDto() { Id = id, Text = tag, Color = color });
    }

    return tags;
  }

  public GetTagCountDbDto GetTagCount()
  {
    using SqliteConnection connection = new(_connectionString);
    connection.Open();

    string query = "SELECT COUNT(*) FROM Tags";
    using SqliteCommand command = new(query, connection);
    return new GetTagCountDbDto() { Count = Convert.ToInt32(command.ExecuteScalar()) };
  }

  public async Task<IEnumerable<TagDbDto>> GetUncachedTags(CachedTagDbDto cachedTagDbDto)
  {
    var cachedIds = cachedTagDbDto.Ids;

    await using SqliteConnection connection = new(_connectionString);
    await connection.OpenAsync();

    string query = "SELECT * FROM Tags";
    int cachedCount = cachedIds.Count;

    if (cachedCount > 0)
    {
      var range = Enumerable.Range(0, cachedCount).Select(index => $"@{index}");
      string inClause = $"WHERE id NOT IN ({string.Join(", ", range)})";
      query = $"{query} {inClause}";
    }

    await using SqliteCommand command = new(query, connection);
    for (int index = 0; index < cachedCount; index++)
      command.Parameters.AddWithValue($"@{index}", cachedIds[index]);

    List<TagDbDto> tagDbDtos = new();
    await using SqliteDataReader reader = command.ExecuteReader();
    while (await reader.ReadAsync())
    {
      Guid id = GetReaderValue<Guid>(reader, "id");
      string text = GetReaderValue<string>(reader, "text")!;
      int color = GetReaderValue<int>(reader, "color");
      tagDbDtos.Add(new() { Id = id, Text = text, Color = color });
    }

    return tagDbDtos;
  }

  public async Task<IEnumerable<TagDbDto>> GetAllTags()
  {
    await using SqliteConnection connection = new(_connectionString);
    await connection.OpenAsync();

    string query = "SELECT * FROM Tags";
    await using SqliteCommand command = new(query, connection);

    List<TagDbDto> tagDbDtos = new();
    await using SqliteDataReader reader = command.ExecuteReader();
    while (await reader.ReadAsync())
    {
      Guid id = GetReaderValue<Guid>(reader, "id");
      string text = GetReaderValue<string>(reader, "text")!;
      int color = GetReaderValue<int>(reader, "color");
      tagDbDtos.Add(new() { Id = id, Text = text, Color = color });
    }

    return tagDbDtos;
  }

  public bool AddTag(TagDbDto dto)
  {
    using SqliteConnection connection = new(_connectionString);
    connection.Open();
    string query = "INSERT OR IGNORE INTO Tags (id, text, color) VALUES (@id, @text, @color)";
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@id", dto.Id);
    command.Parameters.AddWithValue("@text", dto.Text);
    command.Parameters.AddWithValue("@color", dto.Color);
    return command.ExecuteNonQuery() > 0;
  }

  public bool AddTagToNote(TagToNoteDbDto dto)
  {
    using SqliteConnection connection = new(_connectionString);
    connection.Open();
    string query = @"INSERT OR IGNORE INTO NotesTags (note_id, tag_id) VALUES (@note_id, @tag_id);";
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@note_id", dto.NoteId);
    command.Parameters.AddWithValue("@tag_id", dto.TagId);
    return command.ExecuteNonQuery() > 0;
  }

  public bool DeleteTag(DeleteTagDbDto dto)
  {
    using SqliteConnection connection = new(_connectionString);
    connection.Open();
    string query = "DELETE FROM Tags WHERE id = @id";
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@id", dto.Id);
    return command.ExecuteNonQuery() > 0;
  }

  public bool DeleteTagFromNote(TagToNoteDbDto dto)
  {
    using SqliteConnection connection = new(_connectionString);
    connection.Open();
    string query = "DELETE FROM NotesTags WHERE note_id = @note_id AND tag_id = @tag_id";
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@note_id", dto.NoteId);
    command.Parameters.AddWithValue("@tag_id", dto.TagId);
    return command.ExecuteNonQuery() > 0;
  }
  #endregion

  #region Helpers
  public static T? GetReaderValue<T>(SqliteDataReader reader, string fieldName, T? nullValue = default) where T : notnull
  {
    int ordinal = reader.GetOrdinal(fieldName);
    return reader.IsDBNull(ordinal)
      ? nullValue
      : typeof(T) switch
      {
        Type t when t == typeof(bool) => (T)(object)reader.GetBoolean(ordinal),
        Type t when t == typeof(byte) => (T)(object)reader.GetByte(ordinal),
        Type t when t == typeof(short) => (T)(object)reader.GetInt16(ordinal),
        Type t when t == typeof(int) => (T)(object)reader.GetInt32(ordinal),
        Type t when t == typeof(long) => (T)(object)reader.GetInt64(ordinal),
        Type t when t == typeof(DateTime) => (T)(object)reader.GetDateTime(ordinal),
        Type t when t == typeof(DateTimeOffset) => (T)(object)reader.GetDateTimeOffset(ordinal),
        Type t when t == typeof(Guid) => (T)(object)new Guid(reader.GetString(ordinal)),
        Type t when t == typeof(string) => (T)(object)reader.GetString(ordinal),
        Type t when t == typeof(double) => (T)(object)reader.GetDouble(ordinal),
        _ => (T)reader[ordinal]
      };
  }
  #endregion

  //TEST
  #region TEST
  public IEnumerable<string> ReadTableData(string tableName)
  {
    using SqliteConnection connection = new(_connectionString);
    connection.Open();

    if (tableName == "Boards")
    {
      string query = "SELECT * FROM Boards";
      using SqliteCommand command = new(query, connection);
      using SqliteDataReader reader = command.ExecuteReader();
      int num = 0;
      while (reader.Read())
      {
        yield return
@$"[ {num++} ]
id || {reader["id"]}
grouped || {reader["grouped"]}
parent || {reader["parent"]}
previous || {reader["previous"]}
name || {reader["name"]}
icon_type || {reader["icon_type"]}
icon_value || {reader["icon_value"]}
";
      }
    }
    else if (tableName == "Notes")
    {
      string query = "SELECT * FROM Notes";
      using SqliteCommand command = new(query, connection);
      using SqliteDataReader reader = command.ExecuteReader();
      int num = 0;
      while (reader.Read())
      {
        string body = (string)reader["body"];
        string subBody = body[..Math.Min(100, body.Length)].ReplaceLineEndings(" ");
        yield return
@$"[ {num++} ]
id || {reader["id"]}
parent || {reader["parent"]}
created || {reader["created"]}
modified || {reader["modified"]}
title || {reader["title"]}
body || {subBody}
background || {reader["background"]}
backdrop || {reader["backdrop"]}
width || {reader["width"]}
height || {reader["height"]}
position_x || {reader["position_x"]}
position_y || {reader["position_y"]}
bookmarked || {reader["bookmarked"]}
trashed || {reader["trashed"]}
";
      }
    }
    else if (tableName == "Tags")
    {
      string query = "SELECT * FROM Tags";
      using SqliteCommand command = new(query, connection);
      using SqliteDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        yield return
@$"[ {reader["text"]} ], ";
      }
    }
    else if (tableName == "NotesTags")
    {
      string query = "SELECT * FROM NotesTags ORDER BY note_id ASC";
      using SqliteCommand command = new(query, connection);
      using SqliteDataReader reader = command.ExecuteReader();
      string id = "";
      while (reader.Read())
      {
        if ((string)reader["note_id"] == id)
          yield return
@$"{reader["tag_id"]}, ";
        else
        {
          id = (string)reader["note_id"];
          yield return
@$"
[ {reader["note_id"]} ]
{reader["tag_id"]}, ";
        }
      }
    }
  }

  public void DropTable(string tableName)
  {
    using SqliteConnection connection = new(_connectionString);
    connection.Open();
    if (tableName == "Boards")
    {
      string query = "DROP TABLE IF EXISTS Boards";
      using SqliteCommand command = new(query, connection);
      command.ExecuteNonQuery();
    }
    else if (tableName == "Notes")
    {
      string query = "DROP TABLE IF EXISTS Notes";
      using SqliteCommand command = new(query, connection);
      command.ExecuteNonQuery();
    }
  }

  public void DeleteTableData(string tableName)
  {
    using SqliteConnection connection = new(_connectionString);
    connection.Open();
    if (tableName == "Boards")
    {
      string query = "DELETE FROM Boards; VACUUM;";
      using SqliteCommand command = new(query, connection);
      command.ExecuteNonQuery();
    }
    else if (tableName == "Notes")
    {
      string query = "DELETE FROM Notes; VACUUM;";
      using SqliteCommand command = new(query, connection);
      command.ExecuteNonQuery();
    }
  }
  #endregion
}