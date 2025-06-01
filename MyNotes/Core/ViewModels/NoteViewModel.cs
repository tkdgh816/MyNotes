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
    //App.Current.GetService<NoteViewModelFactory>().Dispose(Note);
    UnregisterEvents();
  }

  #region Handling Events
  public void RegisterEvents()
  {
    Note.PropertyChanged += OnNoteChanged;
    _noteDebounceTimer.Tick += OnNoteDebounceTimerTick;
  }

  public void UnregisterEvents()
  {
    Note.PropertyChanged -= OnNoteChanged;
    _noteDebounceTimer.Tick -= OnNoteDebounceTimerTick;
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
  DispatcherTimer _noteDebounceTimer = new() { Interval = TimeSpan.FromMilliseconds(500) };

  private HashSet<string> _changedNoteProperties = new();
  private void OnNoteDebounceTimerTick(object? sender, object e)
  {
    Debug.WriteLine("ChangedProperties: " + string.Join(", ", _changedNoteProperties));
    foreach (string changedPropertyName in _changedNoteProperties)
    {
      if (changedPropertyName != nameof(Note.Modified))
        DatabaseService.UpdateNote(Note, changedPropertyName, true);
      OnPropertyChanged(changedPropertyName);
    }
    ClearNotePropertyChangedFlags();
    _noteDebounceTimer.Stop();
  }
  #endregion

  #region Note Properties
  private void OnNoteChanged(object? sender, PropertyChangedEventArgs e)
  {
    string? propertyName = e.PropertyName;
    if (propertyName is null)
      return;
    _noteDebounceTimer.Stop();
    _changedNoteProperties.Add(propertyName);
    _noteDebounceTimer.Start();
  }

  //TEST TOSTRING
  public override string ToString() => Note.ToString();

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
  public Command<bool>? CreateWindowCommand { get; private set; }
  public Command? MinimizeWindowCommand { get; private set; }
  public Command? CloseWindowCommand { get; private set; }
  public Command? ToggleWindowPinCommand { get; private set; }
  public Command<int>? SetWindowBackdropCommand { get; private set; }
  public Command? BookmarkCommand { get; private set; }
  public Command? RemoveCommand { get; private set; }
  public Command<NoteViewModel>? MoveToBoardCommand { get; private set; }
  public Command? RenameTitleCommand { get; private set; }

  private void SetCommands()
  {
    CreateWindowCommand = new((enabled) =>
    {
      if (enabled)
        WindowService.CreateNoteWindow(Note).Activate();
    });

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
      //DatabaseService.UpdateNote(Note, nameof(Note.IsBookmarked), false);
      if (!Note.IsBookmarked)
        WeakReferenceMessenger.Default.Send(new Message(this), Tokens.RemoveUnbookmarkedNote);
    });

    RemoveCommand = new(() =>
    {
      Note.IsTrashed = true;
      //DatabaseService.UpdateNote(Note, nameof(Note.IsTrashed), false);
    });

    MoveToBoardCommand = new((noteViewModel) =>
    {
      WeakReferenceMessenger.Default.Send(new DialogRequestMessage(noteViewModel), Tokens.MoveNoteToBoard);
    });

    RenameTitleCommand = new(async () =>
    {
      AsyncRequestMessage<string> message = new();
      await WeakReferenceMessenger.Default.Send(message, Tokens.RenameNoteTitle);

      string title = await message.Response;
      title = title.Trim();
      if (!string.IsNullOrWhiteSpace(title))
        Note.Title = title;
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
