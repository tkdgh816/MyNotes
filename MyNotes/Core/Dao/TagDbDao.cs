using Microsoft.Data.Sqlite;

using MyNotes.Core.Dto;
using MyNotes.Core.Service;

namespace MyNotes.Core.Dao;
internal class TagDbDao(DatabaseService databaseService) : DbDaoBase
{
  private readonly DatabaseService _databaseService = databaseService;

  public async Task<IEnumerable<TagDto>> GetTags(GetNoteTagsDto dto)
  {
    List<TagDto> tags = new();
    await using SqliteConnection connection = _databaseService.Connection;
    await connection.OpenAsync();

    string query = """
      SELECT * FROM Tags
      INNER JOIN NotesTags ON Tags.id = NotesTags.tag_id
      WHERE NotesTags.note_id = @note_id
      """;
    await using SqliteCommand command = new(query, connection);

    command.Parameters.AddWithValue("@note_id", dto.NoteId);

    await using SqliteDataReader reader = command.ExecuteReader();
    while (await reader.ReadAsync())
    {
      var id = GetReaderValue<Guid>(reader, "id");
      var tag = GetReaderValue<string>(reader, "text")!;
      var color = GetReaderValue<int>(reader, "color");
      tags.Add(new TagDto() { Id = id, Text = tag, Color = color });
    }

    return tags;
  }

  public async Task<IEnumerable<TagDto>> GetAllTags()
  {
    await using SqliteConnection connection = _databaseService.Connection;
    await connection.OpenAsync();

    string query = "SELECT * FROM Tags";
    await using SqliteCommand command = new(query, connection);

    List<TagDto> tagDbDtos = new();
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

  public bool AddTag(TagDto dto)
  {
    using SqliteConnection connection = _databaseService.Connection;
    connection.Open();
    string query = "INSERT OR IGNORE INTO Tags (id, text, color) VALUES (@id, @text, @color)";
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@id", dto.Id);
    command.Parameters.AddWithValue("@text", dto.Text);
    command.Parameters.AddWithValue("@color", dto.Color);
    return command.ExecuteNonQuery() > 0;
  }

  public bool AddTagToNote(TagToNoteDto dto)
  {
    using SqliteConnection connection = _databaseService.Connection;
    connection.Open();
    string query = "INSERT OR IGNORE INTO NotesTags (note_id, tag_id) VALUES (@note_id, @tag_id);";
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@note_id", dto.NoteId);
    command.Parameters.AddWithValue("@tag_id", dto.TagId);
    return command.ExecuteNonQuery() > 0;
  }

  public bool DeleteTag(DeleteTagDto dto)
  {
    using SqliteConnection connection = _databaseService.Connection;
    connection.Open();
    string query = "DELETE FROM Tags WHERE id = @id";
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@id", dto.Id);
    return command.ExecuteNonQuery() > 0;
  }

  public bool DeleteTagFromNote(TagToNoteDto dto)
  {
    using SqliteConnection connection = _databaseService.Connection;
    connection.Open();
    string query = "DELETE FROM NotesTags WHERE note_id = @note_id AND tag_id = @tag_id";
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@note_id", dto.NoteId);
    command.Parameters.AddWithValue("@tag_id", dto.TagId);
    return command.ExecuteNonQuery() > 0;
  }
}
