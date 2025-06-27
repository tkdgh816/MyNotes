namespace MyNotes.Core.Model;

internal class Note : ObservableObject
{
  public Note(NoteId id, BoardId boardId, string title, DateTimeOffset created, DateTimeOffset modified)
  {
    Id = id;
    BoardId = boardId;
    _title = title;
    Created = created;
    _modified = modified;
  }

  public void Initialize()
  {
    PropertyChanged += OnPropertyChanged;
  }

  //TEST TO STRING
  public override string ToString() => Title;

  static readonly HashSet<string> _propertiesAffectingModified = new() { nameof(Title), nameof(Body), nameof(Background), nameof(Backdrop) };
  private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (e.PropertyName is not null && _propertiesAffectingModified.Contains(e.PropertyName))
      Modified = DateTimeOffset.UtcNow;
  }

  public NoteId Id { get; private set; }
  public BoardId BoardId { get; set; }
  public DateTimeOffset Created { get; private set; }

  private DateTimeOffset _modified;
  public DateTimeOffset Modified
  {
    get => _modified;
    private set => SetProperty(ref _modified, value);
  }

  private string _title;
  public string Title
  {
    get => _title;
    set => SetProperty(ref _title, value);
  }

  private string _body = "";
  public string Body
  {
    get => _body;
    set => SetProperty(ref _body, value);
  }

  private Color _background = Colors.LightGoldenrodYellow;
  public Color Background
  {
    get => _background;
    set => SetProperty(ref _background, value);
  }

  private BackdropKind _backdrop = BackdropKind.None;
  public BackdropKind Backdrop
  {
    get => _backdrop;
    set => SetProperty(ref _backdrop, value);
  }

  private SizeInt32 _size = new SizeInt32(300, 300);
  public SizeInt32 Size
  {
    get => _size;
    set => SetProperty(ref _size, value);
  }

  private PointInt32 _position;
  public PointInt32 Position
  {
    get => _position;
    set => SetProperty(ref _position, value);
  }

  private bool _isBookmarked;
  public bool IsBookmarked
  {
    get => _isBookmarked;
    set => SetProperty(ref _isBookmarked, value);
  }

  private bool _isTrashed;
  public bool IsTrashed
  {
    get => _isTrashed;
    set => SetProperty(ref _isTrashed, value);
  }

  public ObservableCollection<Tag> Tags { get; init; } = new();
}

internal enum BackdropKind { None, Acrylic, Mica }

internal enum NoteSortKey { Modified, Created, Title, Tag }