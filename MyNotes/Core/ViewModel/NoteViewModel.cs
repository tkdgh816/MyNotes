using MyNotes.Common.Commands;
using MyNotes.Common.Messaging;
using MyNotes.Core.Dto;
using MyNotes.Core.Model;
using MyNotes.Core.Service;

namespace MyNotes.Core.ViewModel;

internal class NoteViewModel : ViewModelBase
{
  public NoteViewModel(Note note, WindowService windowService, NavigationService navigationService, DatabaseService databaseService, NoteService noteService)
  {
    Note = note;
    _windowService = windowService;
    _navigationService = navigationService;
    _databaseService = databaseService;
    _noteService = noteService;
    RegisterEvents();
    SetCommands();
    RegisterMessengers();
  }

  private readonly WindowService _windowService;
  private readonly NavigationService _navigationService;
  private readonly DatabaseService _databaseService;
  private readonly NoteService _noteService;
  public Note Note { get; private set; }

  private bool _isWindowActive = true;
  public bool IsWindowActive
  {
    get => _isWindowActive;
    set => SetProperty(ref _isWindowActive, value);
  }

  public void Print() => Debug.WriteLine("Print");
  public bool IsNoteInBoard()
  {
    if (_navigationService.CurrentNavigation is NavigationBoard navigationBoard)
    {
      var boardViewModel = App.Current.GetService<BoardViewModelFactory>().Get(navigationBoard);
      if (boardViewModel is not null && boardViewModel.NoteViewModels.Contains(this))
        return true;
    }

    return false;
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing)
    {
      Debug.WriteLine("Disposing... | " + Note.Title);
      UnregisterEvents();
      UnregisterMessengers();
      App.Current.GetService<NoteViewModelFactory>().Close(Note);
      App.Current.GetService<NoteService>().RemoveNoteCache(Note);
    }

    base.Dispose(disposing);
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
  public void GetMainWindow() => _windowService.GetMainWindow().Activate();
  public void CreateWindow() => _windowService.CreateNoteWindow(Note).Activate();
  public void SetWindowTitlebar(UIElement titleBar) => _windowService.SetNoteWindowTitleBar(Note, titleBar);
  private bool _isWindowAlwaysOnTop = false;
  public bool IsWindowAlwaysOnTop
  {
    get => _isWindowAlwaysOnTop;
    set => SetProperty(ref _isWindowAlwaysOnTop, value);
  }

  public void SetWindowRegionRects(RectInt32[]? rects) => _windowService.SetNoteWindowRegionRects(Note, rects);
  #endregion

  #region Timers
  private DispatcherTimer _noteDebounceTimer = new() { Interval = TimeSpan.FromMilliseconds(500) };

  private HashSet<string> _changedNoteProperties = new();
  private NoteUpdateFields _noteUpdateFields = NoteUpdateFields.None;
  private Dictionary<string, NoteUpdateFields> _notePropertyUpdateFieldMap = new()
  {
    { nameof(Note.BoardId), NoteUpdateFields.Parent },
    { nameof(Note.Modified), NoteUpdateFields.Modified },
    { nameof(Note.Title), NoteUpdateFields.Title },
    { nameof(Note.Body), NoteUpdateFields.Body },
    { nameof(Note.Background), NoteUpdateFields.Background },
    { nameof(Note.Backdrop), NoteUpdateFields.Backdrop },
    { nameof(Note.Size), NoteUpdateFields.Width | NoteUpdateFields.Height },
    { nameof(Note.Position), NoteUpdateFields.PositionX | NoteUpdateFields.PositionY },
    { nameof(Note.IsBookmarked), NoteUpdateFields.Bookmarked },
    { nameof(Note.IsTrashed), NoteUpdateFields.Trashed },
  };

  private void OnNoteDebounceTimerTick(object? sender, object e)
  {
    Debug.WriteLine("ChangedProperties: " + string.Join(", ", _changedNoteProperties));
    Debug.WriteLine("UpdateNotesFields: " + _noteUpdateFields);
    _noteService.UpdateNote(Note, _noteUpdateFields);
    foreach (string changedPropertyName in _changedNoteProperties)
      OnPropertyChanged(changedPropertyName);
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
    _noteUpdateFields |= _notePropertyUpdateFieldMap[propertyName];
    _noteDebounceTimer.Start();
  }

  //TEST TOSTRING
  public override string ToString() => Note.ToString();

  public bool IsBodyChanged = false;
  public void UpdateBody(string body) => Note.Body = body;
  public void ForceUpdateNoteProperties()
  {
    _noteService.UpdateNote(Note, NoteUpdateFields.All);
    ClearNotePropertyChangedFlags();
  }

  private void ClearNotePropertyChangedFlags()
  {
    _changedNoteProperties.Clear();
    _noteUpdateFields = NoteUpdateFields.None;
    IsBodyChanged = false;
  }

  public void UpdateNoteTag(string tag)
  {
    if (string.IsNullOrWhiteSpace(tag) || Note.Tags.Contains(tag))
      return;

    Note.Tags.Add(tag);
    _databaseService.AddTag(Note, tag);
  }
  #endregion

  #region Commands
  public Command<bool>? OpenWindowCommand { get; private set; }
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
    OpenWindowCommand = new((enabled) =>
    {
      if (enabled)
        _windowService.CreateNoteWindow(Note).Activate();
    });

    MinimizeWindowCommand = new(() => _windowService.MinimizeNoteWindow(Note));
    CloseWindowCommand = new(() => _windowService.CloseNoteWindow(Note));
    ToggleWindowPinCommand = new(() => _windowService.ToggleNoteWindowPin(Note, IsWindowAlwaysOnTop = !IsWindowAlwaysOnTop));
    SetWindowBackdropCommand = new((backdropKind) =>
    {
      Note.Backdrop = (BackdropKind)backdropKind;
      _windowService.SetNoteWindowBackdrop(Note, (BackdropKind)backdropKind);
    });

    BookmarkCommand = new(() =>
    {
      Note.IsBookmarked = !Note.IsBookmarked;
      if (!Note.IsBookmarked)
        WeakReferenceMessenger.Default.Send(new Message(this), Tokens.RemoveUnbookmarkedNote);
    });

    RemoveCommand = new(() =>
    {
      Note.IsTrashed = true;
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

  private void UnregisterMessengers()
  {
    WeakReferenceMessenger.Default.UnregisterAll(this);
  }
  #endregion
}
