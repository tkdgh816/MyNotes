using System.Text.RegularExpressions;
using System.Threading;

using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Documents;

using MyNotes.Common.Collections;
using MyNotes.Common.Messaging;
using MyNotes.Core.Model;
using MyNotes.Core.Service;
using MyNotes.Core.Shared;

namespace MyNotes.Core.ViewModel;

internal partial class BoardViewModel : ViewModelBase
{
  private readonly NavigationBoard _navigation;
  private readonly SettingsService _settingsService;
  private readonly WindowService _windowService;
  private readonly DialogService _dialogService;
  private readonly NoteService _noteService;
  private readonly NoteViewModelFactory _noteViewModelFactory;

  public BoardViewModel(NavigationBoard navigation, SettingsService settingsService, WindowService windowService, DialogService dialogService, NoteService noteService, NoteViewModelFactory noteViewModelFactory)
  {
    _navigation = navigation;
    _settingsService = settingsService;
    _windowService = windowService;
    _dialogService = dialogService;
    _noteService = noteService;
    _noteViewModelFactory = noteViewModelFactory;

    InitializeSettings();
    GetNotes();
    GetNoteViewModels();
    SetCommands();
    RegisterMessengers();
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing)
    {
      _cancellationTokenSource.Cancel();

      foreach (var noteViewModel in NoteViewModels)
      {
        if (!_windowService.IsNoteWindowActive(noteViewModel.Note))
          noteViewModel.Dispose();
      }
      if (_navigation is NavigationSearch)
      {
        foreach (var note in Notes)
          note.HighlighterRanges.Clear();
      }

      NoteViewModels.Clear();
      Notes.Clear();
      UnregisterMessengers();
      App.Instance.GetService<BoardViewModelFactory>().Remove(_navigation);
    }

    base.Dispose(disposing);
  }

  public SortedCollection<Note> Notes { get; private set; } = null!;
  public IncrementalObservableCollection<NoteViewModel> NoteViewModels { get; private set; } = null!;

  private int _noteCount;
  public int NoteCount
  {
    get => _noteCount;
    set => SetProperty(ref _noteCount, value);
  }

  private bool _isNotesLoading = true;
  public bool IsNotesLoading
  {
    get => _isNotesLoading;
    set => SetProperty(ref _isNotesLoading, value);
  }
  private bool _isChecked = false;
  public bool IsChecked
  {
    get => _isChecked;
    set => SetProperty(ref _isChecked, value);
  }
  public bool DisplayNoteCount { get; private set; }

  private readonly CancellationTokenSource _cancellationTokenSource = new();

  public NoteSortField SortField { get; private set; } = NoteSortField.Created;
  public SortDirection SortDirection { get; private set; } = SortDirection.Ascending;
  public BoardViewStyle ViewStyle { get; private set; } = BoardViewStyle.Grid_320_100;

  public void InitializeSettings()
  {
    var settings = _settingsService.GetBoardSettings();
    SortField = settings.SortField;
    SortDirection = settings.SortDirection;
    ViewStyle = settings.ViewStyle;
    DisplayNoteCount = settings.DisplayNoteCount;
  }

  public void SetBoardSettings(string key, BoardViewStyle viewStyle)
  {
    ViewStyle = viewStyle;
    _settingsService.SetBoardSettings(key, (int)ViewStyle);
  }

  private string _searchPreview = "";

  private async void GetNotes()
  {
    Notes = new(GetNoteComparer());
    var notes = Notes;
    await Task.Run(async () =>
    {
      try
      {
        switch (_navigation)
        {
          case NavigationSearch search:
            await foreach (var note in _noteService.SearchNotesStreamAsync(search.SearchText, cancellationToken: _cancellationTokenSource.Token))
            {
              _searchPreview = note.Preview;
              ChangeSearchPreview(note, search.SearchText);
              notes.Add(note);
            }
            break;
          default:
            await foreach (var note in _noteService.GetNotesStreamAsync(_navigation, sortField: SortField, sortDirection: SortDirection, cancellationToken: _cancellationTokenSource.Token))
              notes.Add(note);
            break;
        }
      }
      catch (OperationCanceledException e)
      {
        Debug.WriteLine(e.Message);
      }
    }, _cancellationTokenSource.Token);
    IsNotesLoading = false;
    NoteCount = Notes.Count;
  }

  private void GetNoteViewModels()
  {
    NoteViewModels = new(LoadMoreViewModels);
  }

  private async Task<IEnumerable<NoteViewModel>> LoadMoreViewModels(uint count)
  {
    List<NoteViewModel> vms = new();

    int completeCount = 0;
    while (IsNotesLoading && completeCount++ <= 100)
      await Task.Delay(100);

    if (!IsNotesLoading)
    {
      int loadedCount = NoteViewModels.Count;
      uint index = 0;
      while (index < count)
      {
        if (loadedCount < Notes.Count)
        {
          vms.Add(_noteViewModelFactory.Resolve(Notes[loadedCount++]));
          index++;
        }
        else
          break;
      }
    }

    return vms;
  }

  private void ChangeSearchPreview(Note note, string searchText)
  {
    int firstIndex = _searchPreview.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase);
    int maxLength = AppStyles.GetNoteViewMaxLength(ViewStyle);
    int length = _searchPreview.Length;
    if (firstIndex >= 0)
    {
      if (length <= maxLength)
        note.Preview = _searchPreview[0..length];
      else if (firstIndex + maxLength >= length)
        note.Preview = "..." + _searchPreview[(length - maxLength)..length];
      else
        note.Preview = "..." + _searchPreview[firstIndex..(firstIndex + maxLength)];

      note.HighlighterRanges.Clear();

      foreach (Match match in Regex.Matches(note.Preview, searchText, RegexOptions.IgnoreCase))
        note.HighlighterRanges.Add(new TextRange(match.Index, searchText.Length));
    }
  }

  public void RefreshSearchPreview()
  {
    if (_navigation is NavigationSearch search)
      foreach (var note in Notes)
        ChangeSearchPreview(note, search.SearchText);
  }

  #region Sort
  private Comparer<Note> GetNoteComparer()
  {
    int direction = SortDirection == SortDirection.Ascending ? 1 : -1;
    return SortField switch
    {
      NoteSortField.Created => Comparer<Note>.Create((x, y) => x.Created.CompareTo(y.Created) * direction),
      NoteSortField.Title => Comparer<Note>.Create((x, y) => x.Title.CompareTo(y.Title) * direction),
      NoteSortField.Modified => Comparer<Note>.Create((x, y) => x.Modified.CompareTo(y.Modified) * direction),
      _ => throw new ArgumentOutOfRangeException("Invalid sort field")
    };
  }

  private void SortNoteViewModels()
  {
    Notes.Reset(GetNoteComparer());
    NoteViewModels.Clear();
  }
  #endregion


  #region Messengers
  private void RegisterMessengers()
  {
    WeakReferenceMessenger.Default.Register<Message<NoteViewModel>, string>(this, Tokens.RemoveNoteFromBookmarks, new((recipient, message) =>
    {
      var noteViewModel = message.Content;
      if (_navigation is NavigationBookmarks)
        NoteViewModels.Remove(noteViewModel);
    }));

    WeakReferenceMessenger.Default.Register<Message<NoteViewModel>, string>(this, Tokens.RemoveNoteFromBoard, new((recipient, message) =>
    {
      var noteViewModel = message.Content;
      NoteViewModels.Remove(noteViewModel);
    }));

    WeakReferenceMessenger.Default.Register<ExtendedRequestMessage<NoteViewModel, bool>, string>(this, Tokens.IsNoteInBoard, new((recipient, message) => message.Reply(!Disposed && NoteViewModels.Contains(message.Request))));
  }

  private void UnregisterMessengers()
  {
    WeakReferenceMessenger.Default.UnregisterAll(this);
  }
  #endregion
}

