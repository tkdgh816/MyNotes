using CommunityToolkit.Mvvm.Messaging;

using MyNotes.Common.Commands;
using MyNotes.Common.Messaging;
using MyNotes.Core.Models;
using MyNotes.Core.Services;

namespace MyNotes.Core.ViewModels;

public class NoteViewModel : ViewModelBase
{
  public NoteViewModel(Note note, WindowService windowService, DatabaseService databaseService)
  {
    Note = note;
    WindowService = windowService;
    DatabaseService = databaseService;
    RegisterEvents();
    SetCommands();
    RegisterMessengers();
  }

  private readonly WindowService WindowService;
  private readonly DatabaseService DatabaseService;

  public Note Note { get; }

  ~NoteViewModel()
  {
    UnregisterEvents();
  }

  #region Handling Events
  public void RegisterEvents()
  {
    Note.PropertyChanged += OnNoteChanged;
    _noteDebounceTimer.Elapsed += OnNoteDebounceTimerElapsed;
  }
  public void UnregisterEvents()
  {
    Note.PropertyChanged -= OnNoteChanged;
    _noteDebounceTimer.Elapsed -= OnNoteDebounceTimerElapsed;
  }
  #endregion

  #region Windows
  public void GetMainWindow() => WindowService.GetMainWindow().Activate();
  public void CreateWindow() => WindowService.CreateNoteWindow(Note).Activate();
  public void SetWindowTitlebar(UIElement titleBar) => WindowService.SetNoteWindowTitleBar(Note, titleBar);
  private bool _isWindowAlwaysOnTop = false;
  public bool IsWindowAlwaysOnTop
  {
    get => _isWindowAlwaysOnTop;
    set => SetProperty(ref _isWindowAlwaysOnTop, value);
  }

  public void SetWindowRegionRects(RectInt32[]? rects) => WindowService.SetNoteWindowRegionRects(Note, rects);
  #endregion

  #region Timers
  Timer _noteDebounceTimer = new(TimeSpan.FromMilliseconds(500)) { AutoReset = false };

  private HashSet<string> _changedNoteProperties = new();
  private void OnNoteDebounceTimerElapsed(object? sender, ElapsedEventArgs e)
  {
    Debug.WriteLine(string.Join(", ", _changedNoteProperties));
    foreach (string changedPropertyName in _changedNoteProperties)
      DatabaseService.UpdateNote(Note, changedPropertyName, true);
    ClearNotePropertyChangedFlags();
  }
  #endregion

  #region Note Properties
  private void OnNoteChanged(object? sender, PropertyChangedEventArgs e)
  {
    string? propertyName = e.PropertyName;
    this.OnPropertyChanged(e.PropertyName);
    if (propertyName is null || propertyName == nameof(Note.Modified))
      return;
    _noteDebounceTimer.Stop();
    _changedNoteProperties.Add(propertyName);
    _noteDebounceTimer.Start();
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
  #endregion

  #region Commands
  public Command? CreateWindowCommand { get; private set; }
  public Command? MinimizeWindowCommand { get; private set; }
  public Command? CloseWindowCommand { get; private set; }
  public Command? ToggleWindowPinCommand { get; private set; }
  public Command<int>? SetWindowBackdropCommand { get; private set; }
  public Command? BookmarkCommand { get; private set; }
  public Command? RemoveCommand { get; private set; }
  public Command<NoteViewModel>? MoveToBoardCommand { get; private set; }

  private void SetCommands()
  {
    CreateWindowCommand = new(() => WindowService.CreateNoteWindow(Note).Activate());
    MinimizeWindowCommand = new(() => WindowService.MinimizeNoteWindow(Note));
    CloseWindowCommand = new(() => WindowService.CloseNoteWindow(Note));
    ToggleWindowPinCommand = new(() => WindowService.ToggleNoteWindowPin(Note, IsWindowAlwaysOnTop = !IsWindowAlwaysOnTop));
    SetWindowBackdropCommand = new((backdropKind) =>
    {
      Note.Backdrop = (BackdropKind)backdropKind;
      WindowService.SetNoteWindowBackdrop(Note, (BackdropKind)backdropKind);
    });

    BookmarkCommand = new(() =>
    {
      Note.IsBookmarked = !Note.IsBookmarked;
      DatabaseService.UpdateNote(Note, nameof(Note.IsBookmarked), false);
      if (!Note.IsBookmarked)
        WeakReferenceMessenger.Default.Send(new Message(this), Tokens.RemoveUnbookmarkedNote);
    });

    RemoveCommand = new(() =>
    {
      Note.IsTrashed = true;
      DatabaseService.UpdateNote(Note, nameof(Note.IsTrashed), false);
    });

    MoveToBoardCommand = new((noteViewModel) =>
    {
      WeakReferenceMessenger.Default.Send(new DialogRequestMessage(noteViewModel), Tokens.MoveNoteToBoard);
    });
  }
  #endregion

  #region Messengers
  private void RegisterMessengers()
  {
    WeakReferenceMessenger.Default.Register<Message, string>(this, Tokens.ResizeNoteWindow, new((recipient, message) =>
    {
      if (message.Sender == Note)
      {
        var size = (SizeInt32)message.Content!;
        if (size.Width > 0 && size.Height > 0)
          Note.Size = size;
      }
    }));

    WeakReferenceMessenger.Default.Register<Message, string>(this, Tokens.MoveNoteWindow, new((recipient, message) =>
    {
      if (message.Sender == Note)
      {
        var position = (PointInt32)message.Content!;
        if (position.X >= 0 && position.Y >= 0)
          Note.Position = position;
      }
    }));
  }
  #endregion
}
