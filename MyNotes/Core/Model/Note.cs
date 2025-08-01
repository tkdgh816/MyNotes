namespace MyNotes.Core.Model;

internal class Note : ObservableObject
{
  public Note(string title, DateTimeOffset modified)
  {
    _title = title;
    _modified = modified;
  }

  public void Initialize()
  {
    PropertyChanged += OnPropertyChanged;
  }

  public override string ToString() => Title;

  static readonly HashSet<string> _propertiesAffectingModified = new() { nameof(Title), nameof(Preview), nameof(Background), nameof(Backdrop) };
  private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (e.PropertyName is not null && _propertiesAffectingModified.Contains(e.PropertyName))
      Modified = DateTimeOffset.UtcNow;
  }

  public required NoteId Id { get; init; }
  public required BoardId BoardId { get; set; }
  public required DateTimeOffset Created { get; init; }

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

  private string _preview = "";
  public string Preview
  {
    get => _preview;
    set => SetProperty(ref _preview, value);
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

internal enum NoteSortField { Created, Modified, Title }