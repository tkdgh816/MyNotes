using Microsoft.Data.Sqlite;
using ToolkitColorHelper = CommunityToolkit.WinUI.Helpers.ColorHelper;

using MyNotes.Core.Models;

namespace MyNotes.Core.Services;

public class DatabaseService
{
  readonly StorageFolder _databaseFolder = ApplicationData.Current.LocalFolder;
  readonly string _connectionString;

  public DatabaseService()
  {
    _connectionString = $@"Data Source={Path.Combine(_databaseFolder.Path, "data.sqlite")}";
    using SqliteConnection connection = new(_connectionString);
    connection.Open();

    using (SqliteCommand command = new(Query.CreateTable_Boards, connection))
      command.ExecuteNonQuery();
    using (SqliteCommand command = new(Query.CreateTable_Notes, connection))
      command.ExecuteNonQuery();
  }

  private static int GetIconValue(Icon icon)
    => icon.IconType switch
    {
      IconType.Glyph => char.ConvertToUtf32(icon.Code, 0),
      IconType.Emoji => ((Emoji)icon).Index,
      _ => 0
    };
  public static Icon GetIcon(IconType iconType, int iconValue)
    => iconType switch
    {
      IconType.Glyph => IconLibrary.FindGlyph(iconValue),
      IconType.Emoji => IconLibrary.FindEmoji(iconValue),
      _ => IconLibrary.FindEmoji(0)
    };

  public void AddBoard(NavigationBoardItem navigation, NavigationBoardItem? previous = null)
  {
    using SqliteConnection connection = new(_connectionString);
    connection.Open();
    using SqliteCommand command0 = new(Query.Insert_Boards, connection);
    command0.Parameters.AddWithValue("@id", navigation.Id);
    command0.Parameters.AddWithValue("@grouped", navigation is NavigationBoardGroupItem);
    command0.Parameters.AddWithValue("@parent", navigation.Parent?.Id);
    command0.Parameters.AddWithValue("@next", DBNull.Value);
    command0.Parameters.AddWithValue("@name", navigation.Name);
    command0.Parameters.AddWithValue("@icon_type", (int)navigation.Icon.IconType);
    command0.Parameters.AddWithValue("@icon_value", GetIconValue(navigation.Icon));
    command0.ExecuteNonQuery();

    if (previous is not null)
    {
      using SqliteCommand command1 = new(Query.Update_Board_Next, connection);
      command1.Parameters.AddWithValue("@id", previous.Id);
      command1.Parameters.AddWithValue("@next", navigation.Id);
      command1.ExecuteNonQuery();
    }
  }

  public void UpdateBoard(NavigationBoardItem navigation, string updateProperty)
  {
    using SqliteConnection connection = new(_connectionString);
    connection.Open();
    if (updateProperty == "Name")
    {
      using SqliteCommand command = new(Query.Update_Board_Name, connection);
      command.Parameters.AddWithValue("@id", navigation.Id);
      command.Parameters.AddWithValue("@name", navigation.Name);
      command.ExecuteNonQuery();
    }
    else if (updateProperty == "Icon")
    {
      using SqliteCommand command = new(Query.Update_Board_Icon, connection);
      command.Parameters.AddWithValue("@id", navigation.Id);
      command.Parameters.AddWithValue("@icon_type", (int)navigation.Icon.IconType);
      command.Parameters.AddWithValue("@icon_value", GetIconValue(navigation.Icon));
      command.ExecuteNonQuery();
    }
  }

  public void UpdateBoardHierarchy(NavigationBoardItem navigation, NavigationBoardItem? next)
  {
    using SqliteConnection connection = new(_connectionString);
    connection.Open();
    string query = @"UPDATE boards SET parent = @parent, next = @next WHERE id = @id";
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@id", navigation.Id);
    command.Parameters.AddWithValue("@parent", navigation.Parent!.Id);
    if (next is null)
      command.Parameters.AddWithValue("@next", DBNull.Value);
    else
      command.Parameters.AddWithValue("@next", next.Id);
    command.ExecuteNonQuery();
  }

  public void AddNote(Note note)
  {
    using SqliteConnection connection = new(_connectionString);
    connection.Open();
    using SqliteCommand command = new(Query.Insert_Notes, connection);
    command.Parameters.AddWithValue("@id", note.Id);
    command.Parameters.AddWithValue("@parent", note.BoardId);
    command.Parameters.AddWithValue("@created", note.Created);
    command.Parameters.AddWithValue("@modified", note.Modified);
    command.Parameters.AddWithValue("@title", note.Title);
    command.Parameters.AddWithValue("@body", note.Body);
    command.Parameters.AddWithValue("@background", note.Background.ToString());
    command.Parameters.AddWithValue("@backdrop", (int)note.Backdrop);
    command.Parameters.AddWithValue("@width", note.Size.Width);
    command.Parameters.AddWithValue("@height", note.Size.Height);
    command.Parameters.AddWithValue("@position_x", note.Position.X);
    command.Parameters.AddWithValue("@position_y", note.Position.Y);
    command.Parameters.AddWithValue("@bookmarked", note.IsBookmarked);
    command.Parameters.AddWithValue("@trashed", note.IsTrashed);
    command.ExecuteNonQuery();
  }

  private Dictionary<string, string> _notePropertyMap = new()
  {
    { "Title", "title" },
    { "Body", "body" },
    { "Background", "background" },
    { "Backdrop", "backdrop" },
    { "Size", "size" },
    { "Position", "position" },
    { "IsBookmarked", "bookmarked" },
    { "IsTrashed", "trashed" },
  };
  public void UpdateNote(Note note, string propertyName)
  {
    List<(string, object)> fields = new() { ("modified", note.Modified) };
    _notePropertyMap.TryGetValue(propertyName, out string? fieldName);
    if (fieldName is null)
      return;

    PropertyInfo? propertyInfo = typeof(Note).GetProperty(propertyName);
    if (propertyInfo is null)
      return;

    object? changedValue = propertyInfo?.GetValue(note);
    if (changedValue is null)
      return;

    if (fieldName == "size")
    {
      fields.Add(("width", ((SizeInt32)changedValue).Width));
      fields.Add(("height", ((SizeInt32)changedValue).Height));
    }
    else if (fieldName == "position")
    {
      fields.Add(("position_x", ((PointInt32)changedValue).X));
      fields.Add(("position_y", ((PointInt32)changedValue).Y));
    }
    else if (fieldName == "background")
      fields.Add(("background", ((Color)changedValue).ToString()));
    else
      fields.Add((fieldName, changedValue));

    string setClause = string.Join(", ", fields.Select(item => $"{item.Item1} = @{item.Item1}"));
    string query = @$"UPDATE notes SET {setClause} WHERE id = @id";

    using SqliteConnection connection = new(_connectionString);
    connection.Open();
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@id", note.Id);
    foreach (var field in fields)
      command.Parameters.AddWithValue($"@{field.Item1}", field.Item2);
    command.ExecuteNonQuery();
  }
  public void UpdateNote(Note note, bool enableModifiedChanged)
  {
    string setClause = enableModifiedChanged ? ", modified = @modified" : "";
    string query = @$"UPDATE notes SET title = @title, body = @body, background = @background, backdrop = @backdrop, width = @width, height = @height, position_x = @position_x, position_y = @position_y, bookmarked = @bookmarked, trashed = @trashed {setClause} WHERE id = @id";
    using SqliteConnection connection = new(_connectionString);
    connection.Open();
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@id", note.Id);
    command.Parameters.AddWithValue("@title", note.Title);
    command.Parameters.AddWithValue("@body", note.Body);
    command.Parameters.AddWithValue("@background", note.Background.ToString());
    command.Parameters.AddWithValue("@backdrop", (int)note.Backdrop);
    command.Parameters.AddWithValue("@width", note.Size.Width);
    command.Parameters.AddWithValue("@height", note.Size.Height);
    command.Parameters.AddWithValue("@position_x", note.Position.X);
    command.Parameters.AddWithValue("@position_y", note.Position.Y);
    command.Parameters.AddWithValue("@bookmarked", note.IsBookmarked);
    command.Parameters.AddWithValue("@trashed", note.IsTrashed);
    if (enableModifiedChanged)
      command.Parameters.AddWithValue("@modified", note.Modified);
    command.ExecuteNonQuery();
  }

  public static T? GetReaderValue<T>(SqliteDataReader reader, string fieldName)
  {
    int ordinal = reader.GetOrdinal(fieldName);
    if (reader.IsDBNull(ordinal))
      return default;
    return typeof(T) switch
    {
      Type t when t == typeof(bool) => (T)(object)reader.GetBoolean(ordinal),
      Type t when t == typeof(byte) => (T)(object)reader.GetByte(ordinal),
      Type t when t == typeof(short) => (T)(object)reader.GetInt16(ordinal),
      Type t when t == typeof(int) => (T)(object)reader.GetInt32(ordinal),
      Type t when t == typeof(long) => (T)(object)reader.GetInt64(ordinal),
      Type t when t == typeof(DateTime) => (T)(object)reader.GetDateTime(ordinal),
      Type t when t == typeof(DateTimeOffset) => (T)(object)reader.GetDateTimeOffset(ordinal),
      Type t when t == typeof(Guid) => (T)(object)reader.GetString(ordinal),
      Type t when t == typeof(string) => (T)(object)reader.GetString(ordinal),
      Type t when t == typeof(double) => (T)(object)reader.GetDouble(ordinal),
      _ => (T)reader[ordinal]
    };
  }

  public void GetNavigationGroup(NavigationBoardGroupItem group)
  {
    Guid? next = Guid.Empty;
    while (next is not null)
    {
      using SqliteConnection connection = new(_connectionString);
      connection.Open();

      string query = (next == Guid.Empty)
        ? "SELECT * FROM Boards WHERE parent = @parent AND next IS NULL"
        : "SELECT * FROM Boards WHERE parent = @parent AND next = @next";
      using SqliteCommand command = new(query, connection);
      command.Parameters.AddWithValue("@parent", group.Id);
      command.Parameters.AddWithValue("@next", next);
      using SqliteDataReader reader = command.ExecuteReader();

      NavigationBoardItem? item = null;
      if (reader.Read())
      {
        Guid id = new(GetReaderValue<string>(reader, "id")!);
        bool grouped = GetReaderValue<bool>(reader, "grouped")!;
        string name = GetReaderValue<string>(reader, "name")!;
        int iconType = GetReaderValue<int>(reader, "icon_type");
        int iconValue = GetReaderValue<int>(reader, "icon_value");
        Icon icon = GetIcon((IconType)iconType, iconValue);
        if (grouped)
        {
          item = new NavigationBoardGroupItem(name, icon, id);
          GetNavigationGroup((NavigationBoardGroupItem)item);
        }
        else
          item = new NavigationBoardItem(name, icon, id);
        group.Insert(0, item);
        next = id;
      }
      else
        next = null;
    }
  }

  public IEnumerable<Note> GetNotes(NavigationBoardItem navigation)
  {
    using SqliteConnection connection = new(_connectionString);
    connection.Open();

    string query = "SELECT * FROM Notes WHERE parent = @parent";
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@parent", navigation.Id);
    using SqliteDataReader reader = command.ExecuteReader();
    while (reader.Read())
    {
      var id = new Guid(GetReaderValue<string>(reader, "id")!);
      var created = GetReaderValue<DateTimeOffset>(reader, "created");
      var modified = GetReaderValue<DateTimeOffset>(reader, "modified");
      var title = GetReaderValue<string>(reader, "title")!;
      var body = GetReaderValue<string>(reader, "body")!;
      var background = ToolkitColorHelper.ToColor(GetReaderValue<string>(reader, "background")!);
      var backdrop = (BackdropKind)GetReaderValue<int>(reader, "backdrop");
      var size = new SizeInt32(GetReaderValue<int>(reader, "width"), GetReaderValue<int>(reader, "height"));
      var position = new PointInt32(GetReaderValue<int>(reader, "position_x"), GetReaderValue<int>(reader, "position_y"));
      var isBookmarked = GetReaderValue<bool>(reader, "bookmarked");
      var isTrashed = GetReaderValue<bool>(reader, "trashed");
      yield return new Note(id, navigation, title, created, modified)
      { Body = body, Background = background, Backdrop = backdrop, Size = size, Position = position, IsBookmarked = isBookmarked, IsTrashed = isTrashed };
    }
  }

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
id: {reader["id"]}
grouped: {reader["grouped"]}
parent: {reader["parent"]}
next: {reader["next"]}
name: {reader["name"]}
icon_type: {reader["icon_type"]}
icon_value: {reader["icon_value"]}
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
id: {reader["id"]}
parent: {reader["parent"]}
created: {reader["created"]}
modified: {reader["modified"]}
title: {reader["title"]}
body: {subBody}
background: {reader["background"]}
backdrop: {reader["backdrop"]}
width: {reader["width"]}
height: {reader["height"]}
position_x: {reader["position_x"]}
position_y: {reader["position_y"]}
bookmarked: {reader["bookmarked"]}
trashed: {reader["trashed"]}
";
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

  private struct Query
  {
    public const string CreateTable_Boards =
@"CREATE TABLE IF NOT EXISTS Boards
(
id            TEXT PRIMARY KEY NOT NULL,
grouped       INTEGER NOT NULL DEFAULT 0,
parent        TEXT NULL,
next          TEXT NULL,
name          TEXT NOT NULL DEFAULT '',
icon_type     INTEGER NOT NULL,
icon_value    INTEGER NOT NULL
)";

    public const string CreateTable_Notes =
@"CREATE TABLE IF NOT EXISTS Notes
(
id            TEXT PRIMARY KEY NOT NULL,
parent        TEXT NOT NULL,
created       TEXT NOT NULL,
modified      TEXT NOT NULL,
title         TEXT NOT NULL DEFAULT '',
body          TEXT NOT NULL DEFAULT '',
background    TEXT NOT NULL,
backdrop      INTEGER NOT NULL,
width         INTEGER NOT NULL,
height        INTEGER NOT NULL,
position_x    INTEGER NOT NULL,
position_y    INTEGER NOT NULL,
bookmarked    INTEGER NOT NULL DEFAULT 0,
trashed       INTEGER NOT NULL DEFAULT 0,
FOREIGN KEY (parent) REFERENCES Boards(id) ON DELETE RESTRICT ON UPDATE CASCADE
)";

    public const string Insert_Boards =
@"INSERT OR IGNORE INTO Boards
(id, grouped, parent, next, name, icon_type, icon_value)
VALUES
(@id, @grouped, @parent, @next, @name, @icon_type, @icon_value)
";

    public const string Update_Board_Icon =
@"UPDATE Boards
SET icon_type = @icon_type, icon_value = @icon_value
WHERE id = @id";

    public const string Update_Board_Next =
@"UPDATE Boards
SET next = @next
WHERE id = @id";

    public const string Update_Board_Name =
@"UPDATE Boards
SET name = @name
WHERE id = @id";

    public const string Insert_Notes =
@"INSERT OR IGNORE INTO Notes
(id, parent, created, modified, title, body, background, backdrop, width, height, position_x, position_y, bookmarked, trashed)
VALUES
(@id, @parent, @created, @modified, @title, @body, @background, @backdrop, @width, @height, @position_x, @position_y, @bookmarked, @trashed)
";
  }
}
