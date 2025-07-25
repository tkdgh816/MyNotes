﻿using Microsoft.Data.Sqlite;

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

  private void CreateNotesFtsTable(SqliteConnection connection, SqliteTransaction transaction)
  {
    string createQuery = """
      CREATE VIRTUAL TABLE IF NOT EXISTS NotesFts
      USING fts5(id, title, body, tokenize="trigram");
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
          $"""
          [ {num++} ]
          id || {reader["id"]}
          grouped || {reader["grouped"]}
          parent || {reader["parent"]}
          previous || {reader["previous"]}
          name || {reader["name"]}
          icon_type || {reader["icon_type"]}
          icon_value || {reader["icon_value"]}

          """;
      }
    }
    else if (tableName == "Notes")
    {
      string query = "SELECT * FROM Notes LEFT JOIN NotesFts ON Notes.id = NotesFts.id";
      using SqliteCommand command = new(query, connection);
      using SqliteDataReader reader = command.ExecuteReader();
      int num = 0;
      while (reader.Read())
      {
        string body = (string)reader["body"];
        string subBody = body[..Math.Min(100, body.Length)].ReplaceLineEndings(" ");
        yield return
          $"""
          [ {num++} ]
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

          """;
      }
    }
    else if (tableName == "Tags")
    {
      string query = "SELECT * FROM Tags";
      using SqliteCommand command = new(query, connection);
      using SqliteDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        yield return $"[ {reader["text"]} ], ";
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
          yield return $"{reader["tag_id"]}, ";
        else
        {
          id = (string)reader["note_id"];
          yield return
            $"""
            [ {reader["note_id"]} ]
            {reader["tag_id"]}, 
            """;
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