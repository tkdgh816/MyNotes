using CommunityToolkit.Mvvm.Messaging;

using MyNotes.Common.Commands;
using MyNotes.Common.Messaging;
using MyNotes.Core.Models;
using MyNotes.Core.Services;
using MyNotes.Core.Views;

namespace MyNotes.Core.ViewModels;

public class NoteViewModel : ViewModelBase
{
  public NoteViewModel(WindowService windowService, DatabaseService databaseService)
  {
    WindowService = windowService;
    DatabaseService = databaseService;
    Note = WindowService.CurrentNote!;
    SetCommands();
  }

  public WindowService WindowService { get; }
  public DatabaseService DatabaseService { get; }
  public Note Note { get; set; }

  public void GetMainWindow() => WindowService.GetMainWindow().Activate();
  public bool CloseWindow() => WindowService.CloseNoteWindow(Note);
  public void CreateWindow() => WindowService.CreateNoteWindow(Note).Activate();

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
    if (propertyName is null || propertyName == nameof(Note.Modified))
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

  private bool _isWindowAlwaysOnTop = false;
  public bool IsWindowAlwaysOnTop
  {
    get => _isWindowAlwaysOnTop;
    set => SetProperty(ref _isWindowAlwaysOnTop, value);
  }

  public void UpdateNote(string propertyName)
  {
    DatabaseService.UpdateNote(Note, propertyName, false);
  }

  public void UpdateNoteTag(string tag)
  {
    if (string.IsNullOrWhiteSpace(tag) || Note.Tags.Contains(tag))
      return;

    Note.Tags.Add(tag);
    DatabaseService.AddTag(Note, tag);
  }

  public Command? CreateWindowCommand { get; private set; }
  public Command? BookmarkCommand { get; private set; }
  public Command<NoteViewModel>? MoveToBoardCommand { get; private set; }

  private void SetCommands()
  {
    CreateWindowCommand = new(() => WindowService.CreateNoteWindow(Note).Activate());

    BookmarkCommand = new(() =>
    {
      Note.IsBookmarked = !Note.IsBookmarked;
      DatabaseService.UpdateNote(Note, nameof(Note.IsBookmarked), false);
    });

    MoveToBoardCommand = new((noteViewModel) =>
    {
      WeakReferenceMessenger.Default.Send(new DialogRequestMessage(noteViewModel), Tokens.MoveNoteToBoard);
    });
  }
}
