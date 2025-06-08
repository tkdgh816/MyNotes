using System.Collections.Immutable;

using Microsoft.Data.Sqlite;

using MyNotes.Core.Dto;
using MyNotes.Core.Model;

namespace MyNotes.Core.Service;

internal class DatabaseService
{
  private readonly StorageFolder _databaseFolder = ApplicationData.Current.LocalFolder;
  private readonly string _connectionString;

  public DatabaseService()
  {
    _connectionString = new SqliteConnectionStringBuilder()
    {
      DataSource = Path.Combine(_databaseFolder.Path, "data.sqlite"),
      ForeignKeys = true
    }.ToString();
    using SqliteConnection connection = new(_connectionString);
    connection.Open();

    using (SqliteCommand command = new(Queries.CreateTable.Boards, connection))
      command.ExecuteNonQuery();
    using (SqliteCommand command = new(Queries.CreateTable.Notes, connection))
      command.ExecuteNonQuery();
    using (SqliteCommand command = new(Queries.CreateTable.Tags, connection))
      command.ExecuteNonQuery();
    using (SqliteCommand command = new(Queries.CreateTable.NotesTags, connection))
      command.ExecuteNonQuery();
  }

  #region Board DTO
  public void AddBoard(BoardDbInsertDto dto)
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

    using SqliteCommand insertBoardCommand = new(Queries.Insert.Boards, connection);
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

  private Dictionary<string, object> GetBoardUpdateFieldValue(BoardDbUpdateDto dto)
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

  public void UpdateBoard(BoardDbUpdateDto dto)
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

  public void DeleteBoard(BoardDbDeleteDto dto)
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

  #region Note DTO
  public void AddNote(NoteDbDto noteDbDto)
  {
    using SqliteConnection connection = new(_connectionString);
    connection.Open();
    using SqliteCommand command = new(Queries.Insert.Notes, connection);
    command.Parameters.AddWithValue("@id", noteDbDto.Id);
    command.Parameters.AddWithValue("@parent", noteDbDto.Parent);
    command.Parameters.AddWithValue("@created", noteDbDto.Created);
    command.Parameters.AddWithValue("@modified", noteDbDto.Modified);
    command.Parameters.AddWithValue("@title", noteDbDto.Title);
    command.Parameters.AddWithValue("@body", noteDbDto.Body);
    command.Parameters.AddWithValue("@background", noteDbDto.Background);
    command.Parameters.AddWithValue("@backdrop", noteDbDto.Backdrop);
    command.Parameters.AddWithValue("@width", noteDbDto.Width);
    command.Parameters.AddWithValue("@height", noteDbDto.Height);
    command.Parameters.AddWithValue("@position_x", noteDbDto.PositionX);
    command.Parameters.AddWithValue("@position_y", noteDbDto.PositionY);
    command.Parameters.AddWithValue("@bookmarked", noteDbDto.Bookmarked);
    command.Parameters.AddWithValue("@trashed", noteDbDto.Trashed);
    command.ExecuteNonQuery();
  }

  private Dictionary<string, object> GetNoteUpdateFieldValue(NoteDbUpdateDto dto)
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

  public void UpdateNote(NoteDbUpdateDto dto)
  {
    Dictionary<string, object> fields = GetNoteUpdateFieldValue(dto);

    Debug.WriteLine("DBUpdate: " + dto.UpdateFields);
    if (fields.Count == 0)
      return;

    string setClause = string.Join(", ", fields.Select(field => $"{field.Key} = @{field.Key}"));
    string query = @$"UPDATE Notes SET {setClause} WHERE id = @id";

    using SqliteConnection connection = new(_connectionString);
    connection.Open();
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@id", dto.Id);
    foreach (var field in fields)
      command.Parameters.AddWithValue($"@{field.Key}", field.Value);
    command.ExecuteNonQuery();
  }

  public async Task<IEnumerable<NoteDbDto>> GetNotes(NavigationUserBoard navigation)
  {
    List<NoteDbDto> notes = new();
    await using SqliteConnection connection = new(_connectionString);
    await connection.OpenAsync();

    string query = "SELECT * FROM Notes WHERE parent = @parent AND trashed = 0";
    await using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@parent", navigation.Id);

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
    ImmutableList<string> tags = [.. GetTags(id).Result];

    NoteDbDto noteDbDto = new() { Id = id, Parent = parent, Created = created, Modified = modified, Title = title, Body = body, Background = background, Backdrop = backdrop, Width = width, Height = height, PositionX = positionX, PositionY = positionY, Bookmarked = bookmarked, Trashed = trashed, Tags = tags };
    return noteDbDto;
  }
  #endregion

  #region Tags
  public async Task<IEnumerable<string>> GetTagsAll()
  {
    List<string> tags = new();
    await using SqliteConnection connection = new(_connectionString);
    await connection.OpenAsync();

    string query = "SELECT * FROM Tags";
    await using SqliteCommand command = new(query, connection);

    await using SqliteDataReader reader = command.ExecuteReader();
    while (await reader.ReadAsync())
      tags.Add(GetReaderValue<string>(reader, "tag")!);

    return tags;
  }

  public async Task<IEnumerable<string>> GetTags(Note note)
  {
    List<string> tags = new();
    await using SqliteConnection connection = new(_connectionString);
    await connection.OpenAsync();
    string query = "SELECT tag FROM NotesTags WHERE id = @id ORDER BY tag ASC";

    await using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@id", note.Id);

    await using SqliteDataReader reader = command.ExecuteReader();
    while (await reader.ReadAsync())
      tags.Add(GetReaderValue<string>(reader, "tag")!);

    return tags;
  }

  public async Task<IEnumerable<string>> GetTags(Guid id)
  {
    List<string> tags = new();
    await using SqliteConnection connection = new(_connectionString);
    await connection.OpenAsync();
    string query = "SELECT tag FROM NotesTags WHERE id = @id ORDER BY tag ASC";

    await using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@id", id);

    await using SqliteDataReader reader = command.ExecuteReader();
    while (await reader.ReadAsync())
      tags.Add(GetReaderValue<string>(reader, "tag")!);

    return tags;
  }

  public bool AddTag(string tag)
  {
    using SqliteConnection connection = new(_connectionString);
    connection.Open();
    string query = "INSERT OR IGNORE INTO Tags (tag) VALUES (@tag)";
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@tag", tag);
    return command.ExecuteNonQuery() > 0;
  }

  public bool AddTag(Note note, string tag)
  {
    using SqliteConnection connection = new(_connectionString);
    connection.Open();
    string query =
@"INSERT OR IGNORE INTO NotesTags (id, tag)
SELECT @id, tag FROM Tags WHERE tag = @tag";
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@id", note.Id);
    command.Parameters.AddWithValue("@tag", tag);
    return command.ExecuteNonQuery() > 0;
  }

  public bool DeleteTag(string tag)
  {
    using SqliteConnection connection = new(_connectionString);
    connection.Open();
    string query = "DELETE FROM Tags WHERE tag = @tag";
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@tag", tag);
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
@$"[ {reader["tag"]} ], ";
      }
    }
    else if (tableName == "NotesTags")
    {
      string query = "SELECT * FROM NotesTags ORDER BY id ASC, tag ASC";
      using SqliteCommand command = new(query, connection);
      using SqliteDataReader reader = command.ExecuteReader();
      string id = "";
      while (reader.Read())
      {
        if ((string)reader["id"] == id)
          yield return
@$"{reader["tag"]}, ";
        else
        {
          id = (string)reader["id"];
          yield return
@$"
[ {reader["id"]} ]
{reader["tag"]}, ";
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

  #region Queries
  private static class Queries
  {
    internal static class CreateTable
    {
      public const string Boards =
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

      public const string Notes =
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

      public const string Tags =
  @"CREATE TABLE IF NOT EXISTS Tags (tag TEXT PRIMARY KEY NOT NULL)";

      public const string NotesTags =
  @"CREATE TABLE IF NOT EXISTS NotesTags
(
id            TEXT NOT NULL,
tag           TEXT NOT NULL,
PRIMARY KEY (id, tag),
FOREIGN KEY (id) REFERENCES Notes(id) ON DELETE CASCADE ON UPDATE CASCADE,
FOREIGN KEY (tag) REFERENCES Tags(tag) ON DELETE CASCADE ON UPDATE CASCADE
)";
    }

    internal static class Insert
    {
      public const string Boards =
  @"INSERT OR IGNORE INTO Boards
(id, grouped, parent, previous, name, icon_type, icon_value)
VALUES
(@id, @grouped, @parent, @previous, @name, @icon_type, @icon_value)
";

      public const string Notes =
  @"INSERT OR IGNORE INTO Notes
(id, parent, created, modified, title, body, background, backdrop, width, height, position_x, position_y, bookmarked, trashed)
VALUES
(@id, @parent, @created, @modified, @title, @body, @background, @backdrop, @width, @height, @position_x, @position_y, @bookmarked, @trashed)
";
    }

    internal static class Update
    {
      public const string Board_Icon =
  @"UPDATE Boards
SET icon_type = @icon_type, icon_value = @icon_value
WHERE id = @id";

      public const string Board_Previous =
  @"UPDATE Boards
SET previous = @previous
WHERE id = @id";

      public const string Board_Parent =
@"UPDATE Boards
SET parent = @parent
WHERE id = @id";

      public const string Board_Name =
  @"UPDATE Boards
SET name = @name
WHERE id = @id";
    }

    internal static class Delete
    {

    }
  }
  #endregion
}