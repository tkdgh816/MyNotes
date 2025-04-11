using Microsoft.Data.Sqlite;

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

    using (SqliteCommand command = new(Query.CreateGroupsTable, connection))
      command.ExecuteNonQuery();
  }

  private struct Query
  {
    public const string CreateGroupsTable =
      @"CREATE TABLE IF NOT EXISTS groups
        (
          id          TEXT PRIMARY KEY NOT NULL,
          parent_id   TEXT NULL,
          next_id     TEXT NULL,
          name        TEXT NOT NULL DEFAULT '',
          icon_type   INTEGER NOT NULL,
          icon_value  INTEGER NOT NULL
        )";
  }
}
