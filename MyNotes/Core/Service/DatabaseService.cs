using Microsoft.Data.Sqlite;

using Col = MyNotes.Core.Shared.DatabaseSettings.Column;
using Db = MyNotes.Core.Shared.DatabaseSettings;
using Tbl = MyNotes.Core.Shared.DatabaseSettings.Table;

namespace MyNotes.Core.Service;

internal class DatabaseService
{
  private readonly StorageFolder _localFolder = ApplicationData.Current.LocalFolder;
  private readonly string _connectionString;

  public DatabaseService()
  {
    var databaseFolder = _localFolder.CreateFolderAsync(Db.Repository.DirectoryName, CreationCollisionOption.OpenIfExists).GetAwaiter().GetResult();
    _connectionString = new SqliteConnectionStringBuilder()
    {
      DataSource = Path.Combine(databaseFolder.Path, Db.Repository.FileName),
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
    string createQuery = $"""
      CREATE TABLE IF NOT EXISTS {Tbl.Boards}      
      (
      {Col.Id}         TEXT PRIMARY KEY NOT NULL,
      {Col.Grouped}    INTEGER NOT NULL DEFAULT 0,
      {Col.Parent}     TEXT NULL NOT NULL,
      {Col.Previous}   TEXT NULL,
      {Col.Name}       TEXT NOT NULL DEFAULT '',
      {Col.IconType}   INTEGER NOT NULL,
      {Col.IconValue}  INTEGER NOT NULL
      )
      """;

    using SqliteCommand command = new(createQuery, connection, transaction);
    command.ExecuteNonQuery();
  }

  private void CreateNotesTable(SqliteConnection connection, SqliteTransaction transaction)
  {
    string createQuery = $"""
      CREATE TABLE IF NOT EXISTS {Tbl.Notes}
      (
      {Col.Id}         TEXT PRIMARY KEY NOT NULL,
      {Col.Parent}     TEXT NOT NULL,
      {Col.Created}    TEXT NOT NULL,
      {Col.Modified}   TEXT NOT NULL,
      {Col.Title}      TEXT NOT NULL DEFAULT '',
      {Col.Body}       TEXT NOT NULL DEFAULT '',
      {Col.Preview}    TEXT NOT NULL DEFAULT '',
      {Col.Background} TEXT NOT NULL,
      {Col.Backdrop}   INTEGER NOT NULL,
      {Col.Width}      INTEGER NOT NULL,
      {Col.Height}     INTEGER NOT NULL,
      {Col.PositionX}  INTEGER NOT NULL,
      {Col.PositionY}  INTEGER NOT NULL,
      {Col.Bookmarked} INTEGER NOT NULL DEFAULT 0,
      {Col.Trashed}    INTEGER NOT NULL DEFAULT 0
      )
      """;

    using SqliteCommand command = new(createQuery, connection, transaction);
    command.ExecuteNonQuery();
  }

  private void CreateTagsTable(SqliteConnection connection, SqliteTransaction transaction)
  {
    string createQuery = $"""
      CREATE TABLE IF NOT EXISTS {Tbl.Tags}
      (
      {Col.Id}       TEXT PRIMARY KEY NOT NULL,
      {Col.Text}     TEXT NOT NULL UNIQUE,
      {Col.Color}    INTEGER NOT NULL
      )
      """;

    using SqliteCommand command = new(createQuery, connection, transaction);
    command.ExecuteNonQuery();
  }

  private void CreateNotesTagsTable(SqliteConnection connection, SqliteTransaction transaction)
  {
    string createQuery = $"""
      CREATE TABLE IF NOT EXISTS {Tbl.NotesTags}
      (
      {Col.NoteId}   TEXT NOT NULL,
      {Col.TagId}    TEXT NOT NULL,
      PRIMARY KEY ({Col.NoteId}, {Col.TagId}),
      FOREIGN KEY ({Col.NoteId}) REFERENCES {Tbl.Notes}({Col.Id}) ON DELETE CASCADE ON UPDATE CASCADE,
      FOREIGN KEY ({Col.TagId}) REFERENCES {Tbl.Tags}({Col.Id}) ON DELETE CASCADE ON UPDATE CASCADE
      )
      """;

    using SqliteCommand command = new(createQuery, connection, transaction);
    command.ExecuteNonQuery();
  }
}