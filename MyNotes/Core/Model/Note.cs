using Microsoft.UI.Xaml.Documents;

namespace MyNotes.Core.Model;

internal class Note(string title, DateTimeOffset modified) : ObservableObject
{
  public void Initialize()
  {
    PropertyChanged += OnPropertyChanged;
  }

  public override string ToString() => Title;

  static readonly HashSet<string> _propertiesAffectingModified = new() { nameof(Title), nameof(Body), nameof(Background), nameof(Backdrop) };
  private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (e.PropertyName is not null && _propertiesAffectingModified.Contains(e.PropertyName))
      Modified = DateTimeOffset.UtcNow;
  }

  public required NoteId Id { get; init; }
  public required BoardId BoardId { get; set; }
  public required DateTimeOffset Created { get; init; }

  public DateTimeOffset Modified
  {
    get => field;
    private set => SetProperty(ref field, value);
  } = modified;

  public string Title
  {
    get => field;
    set => SetProperty(ref field, value);
  } = title;

  public string Body
  {
    get;
    set => SetProperty(ref field, value);
  } = "";

  public string Preview
  {
    get;
    set => SetProperty(ref field, value);
  } = "";

  public List<TextRange> HighlighterRanges { get; } = new();

  public Color Background
  {
    get => field;
    set => SetProperty(ref field, value);
  } = Colors.LightGoldenrodYellow;

  public BackdropKind Backdrop
  {
    get;
    set => SetProperty(ref field, value);
  } = BackdropKind.None;

  public SizeInt32 Size
  {
    get => field;
    set => SetProperty(ref field, value);
  } = new SizeInt32(300, 300);

  public PointInt32 Position
  {
    get => field;
    set => SetProperty(ref field, value);
  }

  public bool IsBookmarked
  {
    get;
    set => SetProperty(ref field, value);
  }

  public bool IsTrashed
  {
    get;
    set => SetProperty(ref field, value);
  }

  public ObservableCollection<Tag> Tags { get; init; } = new();
}

internal enum BackdropKind { None, Acrylic, Mica }

internal enum NoteSortField { Created, Modified, Title }