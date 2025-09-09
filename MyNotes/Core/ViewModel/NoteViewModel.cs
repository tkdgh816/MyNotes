using MyNotes.Common.Messaging;
using MyNotes.Core.Dto;
using MyNotes.Core.Model;
using MyNotes.Core.Service;
using MyNotes.Core.Shared;

namespace MyNotes.Core.ViewModel;

internal partial class NoteViewModel : ViewModelBase
{
  public Note Note { get; }
  private readonly WindowService _windowService;
  private readonly DialogService _dialogService;
  private readonly NoteService _noteService;
  private readonly TagService _tagService;

  public NoteViewModel(Note note, WindowService windowService, DialogService dialogService, NoteService noteService, TagService tagService)
  {
    Note = note;
    _windowService = windowService;
    _dialogService = dialogService;
    _noteService = noteService;
    _tagService = tagService;
    SetNotePropertyUpdateFieldMap();
    RegisterEvents();
    SetCommands();
    RegisterMessengers();
  }

  public bool IsNoteInBoard()
  {
    ExtendedRequestMessage<NoteViewModel, bool> message = new(this, false);
    WeakReferenceMessenger.Default.Send(message, Tokens.IsNoteInBoard);
    return message.Response;
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing)
    {
      UnregisterEvents();
      UnregisterMessengers();
      App.Instance.GetService<NoteViewModelFactory>().Remove(Note);
    }

    base.Dispose(disposing);
  }

  #region Handling Events
  public void RegisterEvents()
  {
    Note.PropertyChanged += OnNoteChanged;
    Note.Tags.CollectionChanged += OnNoteTagsCollectionChanged;
    _noteDebounceTimer.Tick += OnNoteDebounceTimerTick;
  }

  public void UnregisterEvents()
  {
    Note.PropertyChanged -= OnNoteChanged;
    Note.Tags.CollectionChanged -= OnNoteTagsCollectionChanged;
    _noteDebounceTimer.Tick -= OnNoteDebounceTimerTick;
  }
  #endregion

  #region Windows
  public void GetMainWindow() => _windowService.GetMainWindow().Activate();
  public void CreateWindow() => _windowService.GetNoteWindow(Note).Activate();
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
  private static readonly Dictionary<string, NoteUpdateFields> _notePropertyUpdateFieldMapInBoard = new()
    {
      { nameof(Note.BoardId), NoteUpdateFields.Parent },
      { nameof(Note.Modified), NoteUpdateFields.Modified },
      { nameof(Note.Title), NoteUpdateFields.Title },
      { nameof(Note.IsBookmarked), NoteUpdateFields.Bookmarked },
      { nameof(Note.IsTrashed), NoteUpdateFields.Trashed },
    };
  private static readonly Dictionary<string, NoteUpdateFields> _notePropertyUpdateFieldMapInNote = new()
    {
      { nameof(Note.BoardId), NoteUpdateFields.Parent },
      { nameof(Note.Modified), NoteUpdateFields.Modified },
      { nameof(Note.Title), NoteUpdateFields.Title },
      { nameof(Note.Body), NoteUpdateFields.Body },
      { nameof(Note.Preview), NoteUpdateFields.Preview },
      { nameof(Note.Background), NoteUpdateFields.Background },
      { nameof(Note.Backdrop), NoteUpdateFields.Backdrop },
      { nameof(Note.Size), NoteUpdateFields.Width | NoteUpdateFields.Height },
      { nameof(Note.Position), NoteUpdateFields.PositionX | NoteUpdateFields.PositionY },
      { nameof(Note.IsBookmarked), NoteUpdateFields.Bookmarked },
      { nameof(Note.IsTrashed), NoteUpdateFields.Trashed },
    };
  private Dictionary<string, NoteUpdateFields> _notePropertyUpdateFieldMap = _notePropertyUpdateFieldMapInBoard;

  public void SetNotePropertyUpdateFieldMap(bool isNoteWindowActive = false) => _notePropertyUpdateFieldMap = isNoteWindowActive ? _notePropertyUpdateFieldMapInNote : _notePropertyUpdateFieldMapInBoard;

  private void OnNoteDebounceTimerTick(object? sender, object e)
  {
    _ = UpdateNoteProperties();
    ApplyDebouncedChangesToView();
    _noteDebounceTimer.Stop();
  }

  // Debounce 적용 이후 후속 UI 작업
  private void ApplyDebouncedChangesToView()
  {
  }
  #endregion

  #region Note Properties
  private void OnNoteChanged(object? sender, PropertyChangedEventArgs e)
  {
    string? propertyName = e.PropertyName;
    if (propertyName is null || !_notePropertyUpdateFieldMap.TryGetValue(propertyName, out NoteUpdateFields fieldValue))
      return;
    _noteDebounceTimer.Stop();
    _changedNoteProperties.Add(propertyName);
    _noteUpdateFields |= fieldValue;
    _noteDebounceTimer.Start();
  }

  private void OnNoteTagsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
  {
    switch (e.Action)
    {
      case NotifyCollectionChangedAction.Add:
        if (e.NewItems is not null)
        {
          foreach (Tag tag in e.NewItems)
            _tagService.AddTagToNote(Note, tag);
        }
        break;
      case NotifyCollectionChangedAction.Remove:
        break;
    }
  }

  public void UpdateBody(string body)
  {
    Note.Body = body;
    Note.Preview = body[..Math.Min(body.Length, 5000)];
    _noteService.UpdateSearchDocument(Note, body);
  }

  private async Task UpdateNoteProperties()
  {
    await _noteService.UpdateNote(Note, _noteUpdateFields);
    foreach (string changedPropertyName in _changedNoteProperties)
      OnPropertyChanged(changedPropertyName);
    ClearNotePropertyChangedFlags();
  }

  public async Task ForceUpdateNoteProperties()
  {
    await _noteService.UpdateNote(Note, NoteUpdateFields.All);
    ClearNotePropertyChangedFlags();
  }

  private void ClearNotePropertyChangedFlags()
  {
    _changedNoteProperties.Clear();
    _noteUpdateFields = NoteUpdateFields.None;
  }

  // 본문 저장 파일 가져오기
  public Task<StorageFile> GetFile() => _noteService.GetFile(Note);
  public Task<StorageFile> CreateFile() => _noteService.CreateFile(Note);
  public Task DeleteFile() => _noteService.DeleteFile(Note);
  #endregion

  #region Messengers
  private void RegisterMessengers()
  {
    WeakReferenceMessenger.Default.Register<Message<SizeInt32>, string>(this, Tokens.ResizeNoteWindow, new((recipient, message) =>
    {
      if (message.Sender == Note)
      {
        var size = message.Content;
        if (size.Width > 0 && size.Height > 0)
          Note.Size = size;
      }
    }));

    WeakReferenceMessenger.Default.Register<Message<PointInt32>, string>(this, Tokens.MoveNoteWindow, new((recipient, message) =>
    {
      if (message.Sender == Note)
      {
        var position = message.Content;
        //if (position.X >= 0 && position.Y >= 0)
        Note.Position = position;
      }
    }));
  }

  private void UnregisterMessengers() => WeakReferenceMessenger.Default.UnregisterAll(this);
  #endregion
}
