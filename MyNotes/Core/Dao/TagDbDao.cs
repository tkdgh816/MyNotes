using Microsoft.Data.Sqlite;

using MyNotes.Core.Dto;
using MyNotes.Core.Service;

using Col = MyNotes.Core.Shared.DatabaseSettings.Column;
using Param = MyNotes.Core.Shared.DatabaseSettings.Parameter;
using Tbl = MyNotes.Core.Shared.DatabaseSettings.Table;

namespace MyNotes.Core.Dao;
internal class TagDbDao(DatabaseService databaseService) : DbDaoBase
{
  private readonly DatabaseService _databaseService = databaseService;

  public Task<IEnumerable<TagDto>> GetNoteTags(GetNoteTagsDto dto)
    => Task.Run(async () =>
    {
      List<TagDto> tags = new();
      await using SqliteConnection connection = _databaseService.Connection;
      await connection.OpenAsync();

      string query = $"""
        SELECT * FROM {Tbl.Tags}
        INNER JOIN {Tbl.NotesTags} ON {Tbl.Tags}.id = {Tbl.NotesTags}.{Col.TagId}
        WHERE {Tbl.NotesTags}.{Col.NoteId} = {Param.NoteId}
        """;
      await using SqliteCommand command = new(query, connection);

      command.Parameters.AddWithValue(Param.NoteId, dto.NoteId);

      await using SqliteDataReader reader = await command.ExecuteReaderAsync();
      while (await reader.ReadAsync())
      {
        var id = GetReaderValue<Guid>(reader, Col.Id);
        var text = GetReaderValue<string>(reader, Col.Text)!;
        var color = GetReaderValue<int>(reader, Col.Color);
        tags.Add(new TagDto() { Id = id, Text = text, Color = color });
      }

      return (IEnumerable<TagDto>)tags;
    });

  public Task<IEnumerable<TagDto>> GetAllTags()
    => Task.Run(async () =>
    {
      await using SqliteConnection connection = _databaseService.Connection;
      await connection.OpenAsync();

      string query = $"SELECT * FROM {Tbl.Tags}";
      await using SqliteCommand command = new(query, connection);

      List<TagDto> tagDbDtos = new();
      await using SqliteDataReader reader = await command.ExecuteReaderAsync();
      while (await reader.ReadAsync())
      {
        var id = GetReaderValue<Guid>(reader, Col.Id);
        var text = GetReaderValue<string>(reader, Col.Text)!;
        var color = GetReaderValue<int>(reader, Col.Color);
        tagDbDtos.Add(new() { Id = id, Text = text, Color = color });
      }

      return (IEnumerable<TagDto>)tagDbDtos;
    });

  public bool AddTag(TagDto dto)
  {
    using SqliteConnection connection = _databaseService.Connection;
    connection.Open();
    string query = $"""
      INSERT OR IGNORE INTO {Tbl.Tags}
      ({Col.Id}, {Col.Text}, {Col.Color})
      VALUES
      ({Param.Id}, {Param.Text}, {Param.Color})
      """;
      
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue(Param.Id, dto.Id);
    command.Parameters.AddWithValue(Param.Text, dto.Text);
    command.Parameters.AddWithValue(Param.Color, dto.Color);
    return command.ExecuteNonQuery() > 0;
  }

  public bool AddTagToNote(TagToNoteDto dto)
  {
    using SqliteConnection connection = _databaseService.Connection;
    connection.Open();
    string query = $"""
      INSERT OR IGNORE INTO {Tbl.NotesTags}
      ({Col.NoteId}, {Col.TagId}) 
      VALUES 
      ({Param.NoteId}, {Param.TagId});
      """;
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue(Param.NoteId, dto.NoteId);
    command.Parameters.AddWithValue(Param.TagId, dto.TagId);
    return command.ExecuteNonQuery() > 0;
  }

  public bool DeleteTag(DeleteTagDto dto)
  {
    using SqliteConnection connection = _databaseService.Connection;
    connection.Open();
    string query = $"DELETE FROM {Tbl.Tags} WHERE {Col.Id} = {Param.Id}";
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue(Param.Id, dto.Id);
    return command.ExecuteNonQuery() > 0;
  }

  public bool DeleteTagFromNote(TagToNoteDto dto)
  {
    using SqliteConnection connection = _databaseService.Connection;
    connection.Open();
    string query = $"DELETE FROM {Tbl.NotesTags} WHERE {Col.NoteId} = {Param.NoteId} AND {Col.TagId} = {Param.TagId}";
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue(Param.NoteId, dto.NoteId);
    command.Parameters.AddWithValue(Param.TagId, dto.TagId);
    return command.ExecuteNonQuery() > 0;
  }
}
