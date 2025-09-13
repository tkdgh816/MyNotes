using Microsoft.Data.Sqlite;

using MyNotes.Core.Dto;
using MyNotes.Core.Service;

using Col = MyNotes.Core.Shared.DatabaseSettings.Column;
using Param = MyNotes.Core.Shared.DatabaseSettings.Parameter;
using Tbl = MyNotes.Core.Shared.DatabaseSettings.Table;

namespace MyNotes.Core.Dao;
internal class BoardDbDao(DatabaseService databaseService) : DbDaoBase
{
  private readonly DatabaseService _databaseService = databaseService;

  public void AddBoard(InsertBoardDto dto)
  {
    using SqliteConnection connection = _databaseService.Connection;
    connection.Open();

    object previous = dto.Previous is null ? DBNull.Value : dto.Previous;

    if (dto.Next is not null)
    {
      string updateNextQuery = $"UPDATE {Tbl.Boards} SET {Col.Previous} = {Param.Previous} WHERE {Col.Id} = {Param.Id}";
      using SqliteCommand updateNextCommand = new(updateNextQuery, connection);
      updateNextCommand.Parameters.AddWithValue(Param.Id, dto.Next);
      updateNextCommand.Parameters.AddWithValue(Param.Previous, dto.Id);
      updateNextCommand.ExecuteNonQuery();
    }

    string insertBoardQuery = $"""
      INSERT OR IGNORE INTO {Tbl.Boards}    
      ({Col.Id}, {Col.Grouped}, {Col.Parent}, {Col.Previous}, {Col.Name}, {Col.IconType}, {Col.IconValue})
      VALUES
      ({Param.Id}, {Param.Grouped}, {Param.Parent}, {Param.Previous}, {Param.Name}, {Param.IconType}, {Param.IconValue})
      
      """;
    using SqliteCommand insertBoardCommand = new(insertBoardQuery, connection);
    insertBoardCommand.Parameters.AddWithValue(Param.Id, dto.Id);
    insertBoardCommand.Parameters.AddWithValue(Param.Grouped, dto.Grouped);
    insertBoardCommand.Parameters.AddWithValue(Param.Parent, dto.Parent);
    insertBoardCommand.Parameters.AddWithValue(Param.Previous, previous);
    insertBoardCommand.Parameters.AddWithValue(Param.Name, dto.Name);
    insertBoardCommand.Parameters.AddWithValue(Param.IconType, dto.IconType);
    insertBoardCommand.Parameters.AddWithValue(Param.IconValue, dto.IconValue);
    insertBoardCommand.ExecuteNonQuery();
  }

  public async Task<IEnumerable<BoardDto>> GetBoards()
  {
    List<BoardDto> boards = new();
    await using SqliteConnection connection = _databaseService.Connection;
    await connection.OpenAsync();

    string query = $"SELECT * FROM {Tbl.Boards}";
    await using SqliteCommand command = new(query, connection);
    await using SqliteDataReader reader = command.ExecuteReader();
    while (await reader.ReadAsync())
    {
      Guid id = GetReaderValue<Guid>(reader, Col.Id)!;
      bool grouped = GetReaderValue<bool>(reader, Col.Grouped)!;
      Guid parent = GetReaderValue<Guid>(reader, Col.Parent)!;
      Guid previous = GetReaderValue<Guid>(reader, Col.Previous)!;
      string name = GetReaderValue<string>(reader, Col.Name)!;
      int iconType = GetReaderValue<int>(reader, Col.IconType);
      int iconValue = GetReaderValue<int>(reader, Col.IconValue);
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
      fields.Add(Param.Parent, dto.Parent);
    if (updateFields.HasFlag(BoardUpdateFields.Previous))
      fields.Add(Param.Previous, dto.Previous is null ? DBNull.Value : dto.Previous);
    if (updateFields.HasFlag(BoardUpdateFields.Name) && dto.Name is not null)
      fields.Add(Param.Name, dto.Name);
    if (updateFields.HasFlag(BoardUpdateFields.IconType) && dto.IconType is not null)
      fields.Add(Param.IconType, dto.IconType);
    if (updateFields.HasFlag(BoardUpdateFields.IconValue) && dto.IconValue is not null)
      fields.Add(Param.IconValue, dto.IconValue);
    return fields;
  }

  public void UpdateBoard(UpdateBoardDto dto)
  {
    Dictionary<string, object> fields = GetBoardUpdateFieldValue(dto);

    if (fields.Count == 0)
      return;

    string setClause = string.Join(", ", fields.Select(field => $"{field.Key} = @{field.Key}"));
    string query = $"UPDATE {Tbl.Boards} SET {setClause} WHERE {Col.Id} = {Param.Id}";

    using SqliteConnection connection = _databaseService.Connection;
    connection.Open();
    using SqliteCommand command = new(query, connection);
    command.Parameters.AddWithValue(Param.Id, dto.Id);
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

    string getPreviousQuery = $"SELECT {Col.Previous} from {Tbl.Boards} WHERE {Col.Id} = {Param.Id}";
    using SqliteCommand getPreviousCommand = new(getPreviousQuery, connection);
    getPreviousCommand.Parameters.AddWithValue(Param.Id, dto.Id);
    using SqliteDataReader getPreviousReader = getPreviousCommand.ExecuteReader();
    if (getPreviousReader.Read())
    {
      Guid dbPrevious = GetReaderValue<Guid>(getPreviousReader, Col.Previous);
      previous = dbPrevious == Guid.Empty ? null : dbPrevious;
    }

    string getNextQuery = $"SELECT {Col.Id} from {Tbl.Boards} WHERE {Col.Previous} = {Param.Previous}";
    using SqliteCommand getNextCommand = new(getNextQuery, connection);
    getNextCommand.Parameters.AddWithValue(Param.Previous, dto.Id);
    using SqliteDataReader getNextReader = getNextCommand.ExecuteReader();
    if (getNextReader.Read())
      next = GetReaderValue<Guid>(getNextReader, Col.Id);

    string deleteBoardQuery = $"DELETE from {Tbl.Boards} WHERE {Col.Id} = {Param.Id}";
    using SqliteCommand deleteBoardCommand = new(deleteBoardQuery, connection);
    deleteBoardCommand.Parameters.AddWithValue(Param.Id, dto.Id);
    if (deleteBoardCommand.ExecuteNonQuery() > 0)
    {
      previous = previous is null ? DBNull.Value : previous;
      if (next is not null)
      {
        string updateNextQuery = $"UPDATE {Tbl.Boards} SET {Col.Previous} = {Param.Previous} WHERE {Col.Id} = {Param.Id}";
        using SqliteCommand updateNextCommand = new(updateNextQuery, connection);
        updateNextCommand.Parameters.AddWithValue(Param.Id, next);
        updateNextCommand.Parameters.AddWithValue(Param.Previous, previous);
        updateNextCommand.ExecuteNonQuery();
      }

      string removeNotesQuery = $"UPDATE {Tbl.Notes} SET {Col.Trashed} = 1 WHERE {Col.Parent} = {Param.Parent}";
      using SqliteCommand removeNotesCommand = new(removeNotesQuery, connection);
      removeNotesCommand.Parameters.AddWithValue(Param.Parent, dto.Id);
      removeNotesCommand.ExecuteNonQuery();
    }
  }
}
