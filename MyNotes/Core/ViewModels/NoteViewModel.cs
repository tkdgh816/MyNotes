using MyNotes.Core.Models;
using MyNotes.Core.Services;

namespace MyNotes.Core.ViewModels;

public class NoteViewModel : ViewModelBase
{
  public NoteViewModel(WindowService windowService, DatabaseService databaseService)
  {
    WindowService = windowService;
    DatabaseService = databaseService;
    Note = WindowService.CurrentNote!;
  }

  public WindowService WindowService { get; }
  public DatabaseService DatabaseService { get; }
  public Note Note { get; }

  public void GetMainWindow() => WindowService.GetMainWindow().Activate();
  public bool CloseWindow() => WindowService.CloseNoteWindow(Note);

  public void RegisterNotePropertyChangedEvent()
  {
    Note.PropertyChanged += OnNoteChanged;
    _timer.Elapsed += OnTimerElapsed;
  }
  public void UnregisterNotePropertyChangedEvent()
  {
    Note.PropertyChanged -= OnNoteChanged;
    _timer.Elapsed -= OnTimerElapsed;
  }

  private void OnNoteChanged(object? sender, PropertyChangedEventArgs e)
  {
    string? propertyName = e.PropertyName;
    if (propertyName is null)
      return;
    _timer.Stop();
    _changedNoteProperties.Add(propertyName);
    _timer.Start();
  }

  private HashSet<string> _changedNoteProperties = new();
  private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
  {
    Debug.WriteLine(string.Join(", ", _changedNoteProperties));
    foreach (string changedPropertyName in _changedNoteProperties)
      DatabaseService.UpdateNote(Note, changedPropertyName);
    ClearNotePropertyChangedFlags();
  }

  Timer _timer = new(TimeSpan.FromMilliseconds(500)) { AutoReset = false };
  public void ResizeWindow(int width, int height) => ResizeWindow(new SizeInt32(width, height));
  public void ResizeWindow(SizeInt32 size)
  {
    if (size.Width > 0 && size.Height > 0)
      Note.Size = size;
  }

  public void MoveWindow(int positionX, int positionY) => MoveWindow(new PointInt32(positionX, positionY));
  public void MoveWindow(PointInt32 position)
  {
    if (position.X >= 0 && position.Y >= 0)
      Note.Position = position;
  }

  public bool IsBodyChanged = false;
  public void UpdateBody(string body) => Note.Body = body;
  public void ForceUpdateNoteProperties()
  {
    DatabaseService.UpdateNote(Note, _changedNoteProperties.Count > 0 || IsBodyChanged);
    ClearNotePropertyChangedFlags();
  }

  private void ClearNotePropertyChangedFlags()
  {
    _changedNoteProperties.Clear();
    IsBodyChanged = false;
  }
}
