## 노트 타이틀 변경 => AsncRequestMessage 이용
```cs
WeakReferenceMessenger.Default.Register<AsyncRequestMessage<string>, string>(this, Tokens.RenameNoteTitle, new((recipient, message) =>
{
  async Task<string> t()
  {
    return await View_RenameNoteTitleContentDialog.ShowAsync() == ContentDialogResult.Primary ? ViewModel.NoteTitleToRename : "";
  }
  message.Reply(t());
}));
```

## NavigationView의 빈 공간에서 ContextFlyout 띄우기
```cs
MenuFlyout ContextFlyout = new();
ContextFlyout.Items.Add(new MenuFlyoutItem() { Text = "Context Flyout" });
ScrollViewer MenuItemsHost = (ScrollViewer)((Grid)VisualTreeHelper.GetChild(View_NavigationView, 0)).FindName("MenuItemsScrollViewer");
MenuItemsHost.ContextFlyout = ContextFlyout;
```

## Win2D를 이용한 비트맵 이미지 생성
```cs
CanvasDevice device = CanvasDevice.GetSharedDevice();
CanvasTextLayout layout = new(device, text, new CanvasTextFormat(), float.MaxValue, float.MaxValue);
float padding = 20.0f;
float width = (float)Math.Ceiling(layout.LayoutBounds.Width);
float height = (float)Math.Ceiling(layout.LayoutBounds.Height);
CanvasRenderTarget renderTarget = new(device, width + padding, height + padding, 96.0f);

using (CanvasDrawingSession ds = renderTarget.CreateDrawingSession())
{
  ds.Clear(Colors.Transparent);
  ds.FillRoundedRectangle(5.0f, 5.0f, width + 10.0f, height + 10.0f, 4.0f, 4.0f, Colors.LightGray);
  ds.DrawRoundedRectangle(5.0f, 5.0f, width + 10.0f, height + 10.0f, 4.0f, 4.0f, Colors.Black, 1.0f);
  ds.DrawText("Text", 10.0f, 10.0f, Colors.Black);
}
var bitmapImage = new BitmapImage();

using (InMemoryRandomAccessStream stream = new())
{
  await renderTarget.SaveAsync(stream, CanvasBitmapFileFormat.Png);
  await bitmapImage.SetSourceAsync(stream);
}
```

## Batch Processing
```cs
// NoteService.cs
public async Task<IEnumerable<Note>> GetNotesBatchAsync(NavigationBoard navigation, int count = -1, int startIndex = -1, NoteSortField? sortField = null, SortDirection? sortDirection = null, CancellationToken cancellationToken = default)
{
  GetNotesDto dto = GetNotesDto(navigation, count, startIndex, sortField, sortDirection);

  return (await _dbDao.GetNotesBatchAsync(dto, cancellationToken)).Select(item => ToNote(item));
}

public async Task<IEnumerable<Note>> SearchNotesBatchAsync(string searchText, NoteSortField? sortField = null, SortDirection? sortDirection = null, CancellationToken cancellationToken = default)
{
  List<GetNoteSearchDto> dtos = new(await _searchDao.GetNoteSearchIdsBatch(searchText, cancellationToken));

  return (await _dbDao.SearchNotesBatchAsync(dtos, cancellationToken)).Select(item => ToNote(item, true));
}
```

```cs
// NoteDbDao.cs
  #region Read: Batch
  public Task<IEnumerable<NoteDto>> GetNotesBatchAsync(GetNotesDto dto, CancellationToken cancellationToken = default) =>
    Task.Run(async () =>
    {
      List<NoteDto> noteDtos = new();
      Dictionary<string, object> fields = GetNoteFieldValue(dto);

      if (fields.Count > 0)
      {
        await using SqliteConnection connection = _databaseService.Connection;
        await connection.OpenAsync(cancellationToken);

        string whereClause = string.Join(" AND ", fields.Select(field => $"{field.Key} = @{field.Key}"));
        string limitCluase = dto.Limit > 0 ? "LIMIT @limit" : "";
        string offsetCluase = dto.Offset > 0 ? "OFFSET @offset" : "";
        string orderByClause = GetOrderByClause(dto.SortField, dto.SortDirection);

        // 필요한 필드만(body 제외) 불러오기
        string query = $"""
        SELECT
        id, parent, created, modified, title, preview, background, backdrop, width, height, position_x, position_y, bookmarked, trashed
        FROM Notes
        WHERE {whereClause} {limitCluase} {offsetCluase} {orderByClause}
        """;
        await using SqliteCommand command = new(query, connection);
        foreach (var field in fields)
          command.Parameters.AddWithValue($"@{field.Key}", field.Value);
        if (dto.Limit > 0)
          command.Parameters.AddWithValue("@limit", dto.Limit);
        if (dto.Offset > 0)
          command.Parameters.AddWithValue("@offset", dto.Offset);

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
          noteDtos.Add(CreateNoteDtoWithoutBody(reader));
      }
      return (IEnumerable<NoteDto>)noteDtos;
    }, cancellationToken);

  public Task<List<NoteDto>> SearchNotesBatchAsync(IList<GetNoteSearchDto> dtos, CancellationToken cancellationToken = default) =>
    Task.Run(async () =>
    {
      List<NoteDto> noteDtos = new();
      int count = dtos.Count;
      if (count > 0)
      {
        await using SqliteConnection connection = _databaseService.Connection;
        await connection.OpenAsync(cancellationToken);

        List<SqliteCommand> commands = new();

        for (int index = 0; index < count; index += _batchSize)
        {
          int paramsCount = Math.Min(_batchSize, count - index);
          string inClause = string.Join(", ", Enumerable.Range(index, paramsCount).Select((num) => $"@id{num}"));

          // 필요한 필드만(priview 제외) 불러오기
          string query = $"""
          SELECT
          id, parent, created, modified, title, body, background, backdrop, width, height, position_x, position_y, bookmarked, trashed
          FROM Notes
          WHERE id IN ({inClause}) AND trashed = @trashed
          """;

          await using SqliteCommand command = new(query, connection);
          command.Parameters.AddWithValue("@trashed", 0);
          for (int paramsIndex = index; paramsIndex < index + paramsCount; paramsIndex++)
            command.Parameters.AddWithValue($"@id{paramsIndex}", dtos[paramsIndex].Id);

          commands.Add(command);
        }

        foreach (var command in commands)
        {
          await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
          while (await reader.ReadAsync(cancellationToken))
            noteDtos.Add(CreateNoteDto(reader));
        }
      }
      return noteDtos;
    }, cancellationToken);
  #endregion
```

## Channel Stream
```cs
// NoteDbDao.cs
  #region Read: Channel Stream
  public IAsyncEnumerable<NoteDto> GetNotesChannelStreamAsync(GetNotesDto dto)
  {
    var channel = Channel.CreateUnbounded<NoteDto>();

    Task.Run(async () =>
    {
      try
      {
        Dictionary<string, object> fields = GetNoteFieldValue(dto);

        if (fields.Count == 0)
          return;

        await using SqliteConnection connection = _databaseService.Connection;
        await connection.OpenAsync();

        string whereClause = string.Join(" AND ", fields.Select(field => $"{field.Key} = @{field.Key}"));
        string limitCluase = dto.Limit > 0 ? "LIMIT @limit" : "";
        string offsetCluase = dto.Offset > 0 ? "OFFSET @offset" : "";
        string orderByClause = GetOrderByClause(dto.SortField, dto.SortDirection);

        // 필요한 필드만(body 제외) 불러오기
        string query = $"""
        SELECT
        id, parent, created, modified, title, preview, background, backdrop, width, height, position_x, position_y, bookmarked, trashed
        FROM Notes
        WHERE {whereClause} {limitCluase} {offsetCluase} {orderByClause}
        """;

        await using SqliteCommand command = new(query, connection);
        foreach (var field in fields)
          command.Parameters.AddWithValue($"@{field.Key}", field.Value);
        if (dto.Limit > 0)
          command.Parameters.AddWithValue("@limit", dto.Limit);
        if (dto.Offset > 0)
          command.Parameters.AddWithValue("@offset", dto.Offset);

        await using SqliteDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
          await channel.Writer.WriteAsync(CreateNoteDtoWithoutBody(reader));
      }
      finally
      {
        channel.Writer.Complete();
      }
    });

    return channel.Reader.ReadAllAsync();
  }

  public IAsyncEnumerable<NoteDto> SearchNotesChannelStreamAsync(IList<GetNoteSearchDto> dtos)
  {
    var channel = Channel.CreateUnbounded<NoteDto>();

    Task.Run(async () =>
    {
      try
      {
        int count = dtos.Count;
        if (count > 0)
        {
          await using SqliteConnection connection = _databaseService.Connection;
          await connection.OpenAsync();

          List<SqliteCommand> commands = new();

          int _batchSize = 10;
          for (int index = 0; index < count; index += _batchSize)
          {
            int paramsCount = Math.Min(_batchSize, count - index);
            string inClause = string.Join(", ", Enumerable.Range(index, paramsCount).Select((num) => $"@id{num}"));

            string query = $"""
            SELECT
            *
            FROM Notes
            WHERE id IN ({inClause}) AND trashed = @trashed
            """;


            await using SqliteCommand command = new(query, connection);
            command.Parameters.AddWithValue("@trashed", 0);
            for (int paramsIndex = index; paramsIndex < index + paramsCount; paramsIndex++)
              command.Parameters.AddWithValue($"@id{paramsIndex}", dtos[paramsIndex].Id);

            commands.Add(command);
          }

          foreach (var command in commands)
          {
            await using SqliteDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
              await channel.Writer.WriteAsync(CreateNoteDto(reader));
          }
        }
      }
      finally
      {
        channel.Writer.Complete();
      }
    });

    return channel.Reader.ReadAllAsync();
  }
  #endregion
```