using Microsoft.Data.Sqlite;

using MyNotes.Core.Dto;
using MyNotes.Core.Service;

namespace MyNotes.Core.Dao;
internal class BoardDao(DatabaseService databaseService) : DaoBase
{
  private readonly DatabaseService _databaseService = databaseService;

  public void AddBoard(InsertBoardDto dto)
  {
    using SqliteConnection connection = _databaseService.Connection;
    connection.Open();

    object previous = dto.Previous is null ? DBNull.Value : dto.Previous;

    if (dto.Next is not null)
    {
      string updateNextQuery = "UPDATE Boards SET previous = @previous WHERE id = @id";
      using SqliteCommand updateNextCommand = new(updateNextQuery, connection);
      updateNextCommand.Parameters.AddWithValue("@id", dto.Next);
      updateNextCommand.Parameters.AddWithValue("@previous", dto.Id);
      updateNextCommand.ExecuteNonQuery();
    }

    string insertBoardQuery = """
      INSERT OR IGNORE INTO Boards    
      (id, grouped, parent, previous, name, icon_type, icon_value)
      VALUES
      (@id, @grouped, @parent, @previous, @name, @icon_type, @icon_value)
      
      """;
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

  public async Task<IEnumerable<BoardDto>> GetBoards()
  {
    List<BoardDto> boards = new();
    await using SqliteConnection connection = _databaseService.Connection;
    await connection.OpenAsync();

    string query = "SELECT * FROM Boards";
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
      boards.Add(new BoardDto() { Id = id, Grouped = grouped, Parent = parent, Previous = previous, Name = name, IconType = iconType, IconValue = iconValue });
    }

    return boards;
  }

  private Dictionary<string, object> GetBoardUpdateFieldValue(UpdateBoardDto dto)
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

  public void UpdateBoard(UpdateBoardDto dto)
  {
    Dictionary<string, object> fields = GetBoardUpdateFieldValue(dto);

    if (fields.Count == 0)
      return;

    string setClause = string.Join(", ", fields.Select(field => $"{field.Key} = @{field.Key}"));
    string query = @$"UPDATE Boards SET {setClause} WHERE id = @id";

    using SqliteConnection connection = _databaseService.Connection;
    connection.Open();
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue("@id", dto.Id);
    foreach (var field in fields)
      command.Parameters.AddWithValue($"@{field.Key}", field.Value);
    command.ExecuteNonQuery();
  }

  public void DeleteBoard(DeleteBoardDto dto)
  {
    using SqliteConnection connection = _databaseService.Connection;
    connection.Open();

    object? previous = null;
    object? next = null;

    string getPreviousQuery = "SELECT previous from boards WHERE id = @id";
    using SqliteCommand getPreviousCommand = new(getPreviousQuery, connection);
    getPreviousCommand.Parameters.AddWithValue("@id", dto.Id);
    using SqliteDataReader getPreviousReader = getPreviousCommand.ExecuteReader();
    if (getPreviousReader.Read())
    {
      Guid dbPrevious = GetReaderValue<Guid>(getPreviousReader, "previous");
      previous = dbPrevious == Guid.Empty ? null : dbPrevious;
    }

    string getNextQuery = "SELECT id from boards WHERE previous = @previous";
    using SqliteCommand getNextCommand = new(getNextQuery, connection);
    getNextCommand.Parameters.AddWithValue("@previous", dto.Id);
    using SqliteDataReader getNextReader = getNextCommand.ExecuteReader();
    if (getNextReader.Read())
      next = GetReaderValue<Guid>(getNextReader, "id");

    string deleteBoardQuery = "DELETE from Boards WHERE id = @id";
    using SqliteCommand deleteBoardCommand = new(deleteBoardQuery, connection);
    deleteBoardCommand.Parameters.AddWithValue("@id", dto.Id);
    if (deleteBoardCommand.ExecuteNonQuery() > 0)
    {
      previous = previous is null ? DBNull.Value : previous;
      if (next is not null)
      {
        string updateNextQuery = "UPDATE Boards SET previous = @previous WHERE id = @id";
        using SqliteCommand updateNextCommand = new(updateNextQuery, connection);
        updateNextCommand.Parameters.AddWithValue("@id", next);
        updateNextCommand.Parameters.AddWithValue("@previous", previous);
        updateNextCommand.ExecuteNonQuery();
      }

      string removeNotesQuery = "UPDATE Notes SET trashed = 1 WHERE parent = @parent";
      using SqliteCommand removeNotesCommand = new(removeNotesQuery, connection);
      removeNotesCommand.Parameters.AddWithValue("@parent", dto.Id);
      removeNotesCommand.ExecuteNonQuery();
    }
  }
}
