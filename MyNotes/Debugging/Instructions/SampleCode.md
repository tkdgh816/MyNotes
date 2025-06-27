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
```ㅁㅁㅁ``` 가나다