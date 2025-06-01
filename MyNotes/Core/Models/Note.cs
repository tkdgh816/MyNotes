namespace MyNotes.Core.Models;

public class Note : ObservableObject
{
  public Note(Guid id, Guid boardId, string title, DateTimeOffset created, DateTimeOffset modified)
  {
    Id = id;
    BoardId = boardId;
    _title = title;
    Created = created;
    Modified = modified;
  }

  public void Initialize()
  {
    PropertyChanged += OnPropertyChanged;
  }

  ~Note()
  {
    PropertyChanged -= OnPropertyChanged;
  }

  //TEST TO STRING
  public override string ToString() => Title;

  static readonly HashSet<string> _propertiesAffectingModified = new() { nameof(Title), nameof(Body), nameof(Background), nameof(Backdrop), nameof(Size), nameof(Position) };
  private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (e.PropertyName is not null && _propertiesAffectingModified.Contains(e.PropertyName))
      Modified = DateTimeOffset.UtcNow;
  }

  public Guid Id { get; private set; }
  public Guid BoardId { get; set; }
  public DateTimeOffset Created { get; private set; }

  private DateTimeOffset _modified;
  public DateTimeOffset Modified
  {
    get => _modified;
    private set => SetProperty(ref _modified, value);
  }

  //public DateTimeOffset Modified { get; private set; }

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

  public ObservableCollection<string> Tags { get; } = new();

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
}

public enum BackdropKind { None, Acrylic, Mica }

public enum NoteSortKey { Modified, Created, Title, Tag }