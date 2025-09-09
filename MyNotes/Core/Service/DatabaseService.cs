using Microsoft.Data.Sqlite;

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
      ForeignKeys = true,
      DefaultTimeout = 60
    }.ToString();

    CreateTables();
  }

  public SqliteConnection Connection => new(_connectionString);

  private void CreateTables()
  {
    using SqliteConnection connection = Connection;
    connection.Open();
    using SqliteTransaction transaction = connection.BeginTransaction();
    try
    {
      CreateBoardsTable(connection, transaction);
      CreateNotesTable(connection, transaction);
      CreateTagsTable(connection, transaction);
      CreateNotesTagsTable(connection, transaction);

      transaction.Commit();
    }
    catch
    {
      transaction.Rollback();
      Debug.WriteLine("Database transaction failed");
    }
  }

  private void CreateBoardsTable(SqliteConnection connection, SqliteTransaction transaction)
  {
    string createQuery = """
      CREATE TABLE IF NOT EXISTS Boards      
      (
      id              TEXT PRIMARY KEY NOT NULL,
      grouped         INTEGER NOT NULL DEFAULT 0,
      parent          TEXT NULL NOT NULL,
      previous        TEXT NULL,
      name            TEXT NOT NULL DEFAULT '',
      icon_type       INTEGER NOT NULL,
      icon_value      INTEGER NOT NULL
      )
      """;

    using SqliteCommand command = new(createQuery, connection, transaction);
    command.ExecuteNonQuery();
  }

  private void CreateNotesTable(SqliteConnection connection, SqliteTransaction transaction)
  {
    string createQuery = """
      CREATE TABLE IF NOT EXISTS Notes
      (
      id              TEXT PRIMARY KEY NOT NULL,
      parent          TEXT NOT NULL,
      created         TEXT NOT NULL,
      modified        TEXT NOT NULL,
      title           TEXT NOT NULL DEFAULT '',
      body            TEXT NOT NULL DEFAULT '',
      preview         TEXT NOT NULL DEFAULT '',
      background      TEXT NOT NULL,
      backdrop        INTEGER NOT NULL,
      width           INTEGER NOT NULL,
      height          INTEGER NOT NULL,
      position_x      INTEGER NOT NULL,
      position_y      INTEGER NOT NULL,
      bookmarked      INTEGER NOT NULL DEFAULT 0,
      trashed         INTEGER NOT NULL DEFAULT 0
      )
      """;

    using SqliteCommand command = new(createQuery, connection, transaction);
    command.ExecuteNonQuery();
  }

  private void CreateTagsTable(SqliteConnection connection, SqliteTransaction transaction)
  {
    string createQuery = """
      CREATE TABLE IF NOT EXISTS Tags
      (
      id              TEXT PRIMARY KEY NOT NULL,
      text            TEXT NOT NULL UNIQUE,
      color           INTEGER NOT NULL
      )
      """;

    using SqliteCommand command = new(createQuery, connection, transaction);
    command.ExecuteNonQuery();
  }

  private void CreateNotesTagsTable(SqliteConnection connection, SqliteTransaction transaction)
  {
    string createQuery = """
      CREATE TABLE IF NOT EXISTS NotesTags
      (
      note_id         TEXT NOT NULL,
      tag_id          TEXT NOT NULL,
      PRIMARY KEY (note_id, tag_id),
      FOREIGN KEY (note_id) REFERENCES Notes(id) ON DELETE CASCADE ON UPDATE CASCADE,
      FOREIGN KEY (tag_id) REFERENCES Tags(id) ON DELETE CASCADE ON UPDATE CASCADE
      )
      """;

    using SqliteCommand command = new(createQuery, connection, transaction);
    command.ExecuteNonQuery();
  }
}