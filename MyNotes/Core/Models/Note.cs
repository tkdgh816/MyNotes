namespace MyNotes.Core.Models;

public class Note : ObservableObject, IComparable<Note>
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
    this.PropertyChanged += OnPropertyChanged;
  }

  private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (e.PropertyName != nameof(Modified))
      Modified = DateTimeOffset.UtcNow;
  }

  public int CompareTo(Note? other)
  {
    if (other is null)
      return 1;
    return -Modified.CompareTo(other.Modified);
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