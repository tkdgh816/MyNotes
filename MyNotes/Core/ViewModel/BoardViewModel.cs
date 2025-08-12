using System.Threading;

using Microsoft.UI.Xaml.Documents;

using MyNotes.Common.Collections;
using MyNotes.Common.Commands;
using MyNotes.Common.Messaging;
using MyNotes.Core.Model;
using MyNotes.Core.Service;
using MyNotes.Core.Shared;

namespace MyNotes.Core.ViewModel;

internal class BoardViewModel : ViewModelBase
{
  public NavigationBoard Navigation { get; }
  private readonly SettingsService _settingsService;
  private readonly WindowService _windowService;
  private readonly DialogService _dialogService;
  private readonly NoteService _noteService;
  private readonly NoteViewModelFactory _noteViewModelFactory;

  public IncrementalObservableCollection<NoteViewModel> NoteViewModels { get; private set; } = null!;
  public SortedCollection<Note> Notes { get; private set; } = null!;
  private bool _isCompleted = false;
  private bool _isChecked = false;
  public bool IsChecked
  {
    get => _isChecked;
    set => SetProperty(ref _isChecked, value);
  }

  public BoardViewModel(NavigationBoard navigation, SettingsService settingsService, WindowService windowService, DialogService dialogService, NoteService noteService, NoteViewModelFactory noteViewModelFactory)
  {
    Navigation = navigation;
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
      foreach (var noteViewModel in NoteViewModels)
      {
        if (!_windowService.IsNoteWindowActive(noteViewModel.Note))
          noteViewModel.Dispose();
      }
      if (Navigation is NavigationSearch)
      {
        foreach (var note in Notes)
          note.HighlighterRanges.Clear();
      }

      NoteViewModels.Clear();
      UnregisterMessengers();
      App.Instance.GetService<BoardViewModelFactory>().Remove(Navigation);
    }

    base.Dispose(disposing);
  }

  public NoteSortField SortField { get; private set; } = NoteSortField.Created;
  public SortDirection SortDirection { get; private set; } = SortDirection.Ascending;
  public BoardViewStyle ViewStyle { get; private set; } = BoardViewStyle.Grid_320_100;

  public void InitializeSettings()
  {
    var settings = _settingsService.GetBoardSettings();
    SortField = settings.SortField;
    SortDirection = settings.SortDirection;
    ViewStyle = settings.ViewStyle;
  }

  public void SetBoardSettings(string key, object value) => _settingsService.SetBoardSettings(key, value);

  private void GetNotes()
  {
    Notes = new(GetNoteComparer());
    Task.Run(async () =>
    {
      switch (Navigation)
      {
        case NavigationSearch search:
          await foreach (var note in _noteService.SearchNotesStreamAsync(search.SearchText))
          {
            int firstIndex = note.SearchPreview.IndexOf(search.SearchText, StringComparison.CurrentCultureIgnoreCase);
            int maxLength = GetNoteViewMaxLength();
            int length = note.SearchPreview.Length;
            if (firstIndex >= 0)
            {
              if (firstIndex <= 0 || length <= maxLength)
                note.SearchPreview = note.SearchPreview[0..length];
              else if (firstIndex + maxLength >= length)
                note.SearchPreview = note.SearchPreview[(length - maxLength)..length];
              else
                note.SearchPreview = note.SearchPreview[firstIndex..(firstIndex + maxLength)];

              foreach (int index in FindAllIndexes(note.SearchPreview, search.SearchText))
                note.HighlighterRanges.Add(new TextRange(index, search.SearchText.Length));
            }

            Notes.Add(note);
          }
          break;
        default:
          await foreach (var note in _noteService.GetNotesStreamAsync(Navigation, sortField: SortField, sortDirection: SortDirection))
            Notes.Add(note);
          break;
      }
      Debug.WriteLine("Note Completed");
      _isCompleted = true;
    });
  }

  private static List<int> FindAllIndexes(string source, string keyword)
  {
    var result = new List<int>();

    if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(keyword))
      return result;

    int index = 0;
    while ((index = source.IndexOf(keyword, index, StringComparison.CurrentCultureIgnoreCase)) >= 0)
    {
      result.Add(index);
      index += keyword.Length;
    }

    return result;
  }

  private int GetNoteViewMaxLength() => _settingsService.GetBoardSettings().ViewStyle switch
  {
    BoardViewStyle.Grid_200_0 => 46,
    BoardViewStyle.Grid_240_0 => 87,
    BoardViewStyle.Grid_280_0 => 140,
    BoardViewStyle.Grid_320_0 => 200,
    BoardViewStyle.Grid_360_0 => 270,
    BoardViewStyle.Grid_400_0 => 357,
    BoardViewStyle.Grid_440_0 => 448,

    BoardViewStyle.Grid_200_50 => 115,
    BoardViewStyle.Grid_240_50 => 174,
    BoardViewStyle.Grid_280_50 => 280,
    BoardViewStyle.Grid_320_50 => 360,
    BoardViewStyle.Grid_360_50 => 495,
    BoardViewStyle.Grid_400_50 => 663,
    BoardViewStyle.Grid_440_50 => 784,

    BoardViewStyle.Grid_200_100 => 161,
    BoardViewStyle.Grid_240_100 => 261,
    BoardViewStyle.Grid_280_100 => 420,
    BoardViewStyle.Grid_320_100 => 560,
    BoardViewStyle.Grid_360_100 => 720,
    BoardViewStyle.Grid_400_100 => 918,
    BoardViewStyle.Grid_440_100 => 1120,

    BoardViewStyle.Grid_200_150 => 230,
    BoardViewStyle.Grid_240_150 => 377,
    BoardViewStyle.Grid_280_150 => 525,
    BoardViewStyle.Grid_320_150 => 720,
    BoardViewStyle.Grid_360_150 => 945,
    BoardViewStyle.Grid_400_150 => 1173,
    BoardViewStyle.Grid_440_150 => 1456,

    BoardViewStyle.Grid_200_200 => 299,
    BoardViewStyle.Grid_240_200 => 464,
    BoardViewStyle.Grid_280_200 => 665,
    BoardViewStyle.Grid_320_200 => 880,
    BoardViewStyle.Grid_360_200 => 1170,
    BoardViewStyle.Grid_400_200 => 1479,
    BoardViewStyle.Grid_440_200 => 1792,

    _ => 100
  };


  private void GetNoteViewModels()
  {
    NoteViewModels = new(LoadMoreViewModels);
  }

  Lock lockObj = new();
  private async Task<IEnumerable<NoteViewModel>> LoadMoreViewModels(uint count)
  {
    List<NoteViewModel> vms = new();
    int completeCount = 0;
    while (!_isCompleted && completeCount++ <= 100)
      await Task.Delay(100);

    lock (lockObj)
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
    GetNotes();
    NoteViewModels.Clear();
  }
  #endregion

  #region Commands
  public Command? AddNewNoteCommand { get; private set; }
  public Command<IList<object>>? RemoveNotesCommand { get; private set; }
  public Command<IList<object>>? ShowMoveNoteToBoardDialogCommand { get; private set; }
  public Command<string>? ChangeSortFieldCommand { get; private set; }
  public Command<string>? ChangeSortDirectionCommand { get; private set; }
  public Command? ShowRenameBoardDialogCommand { get; private set; }
  public Command? ShowDeleteBoardDialogCommand { get; private set; }
  public Command<Icon>? ChangeIconCommand { get; private set; }

  private void SetCommands()
  {
    AddNewNoteCommand = new(() =>
    {
      //for (int i = 0; i < 25; i++)
      //{
      Note newNote = _noteService.CreateNote((NavigationUserBoard)Navigation);
      NoteViewModel noteViewModel = _noteViewModelFactory.Resolve(newNote);

      Notes.Add(newNote);
      int index = Notes.IndexOf(newNote);
      if (index <= NoteViewModels.Count)
        NoteViewModels.Insert(index, noteViewModel);

      noteViewModel.CreateWindow();

      //DispatcherQueue.GetForCurrentThread().EnqueueAsync(async () =>
      //{
      //  await Task.Delay(5000);
      //  noteViewModel.CloseWindowCommand?.Execute();
      //});
      //}
    });

    RemoveNotesCommand = new(async (items) =>
    {
      var noteViewModels = items.OfType<NoteViewModel>().ToList();
      int count = noteViewModels.Count;
      if (count == 0)
        return;

      if (await _dialogService.ShowRemoveNoteDialog(count))
      {
        foreach (var noteViewModel in noteViewModels)
        {
          Notes.Remove(noteViewModel.Note);
          noteViewModel.RemoveNoteCommand?.Execute();
        }
      }
    });

    ShowMoveNoteToBoardDialogCommand = new(async (items) =>
    {
      var noteViewModels = items.OfType<NoteViewModel>().ToList();
      int count = noteViewModels.Count;
      if (count == 0)
        return;

      var (dialogResult, boardId) = await _dialogService.ShowMoveNoteToBoardDialog();
      if (dialogResult && boardId is not null)
      {
        if (Navigation is not NavigationUserBoard userBoard || userBoard.Id != boardId)
          foreach (var noteViewModel in noteViewModels)
          {
            Notes.Remove(noteViewModel.Note);
            noteViewModel.MoveNoteToBoardCommand?.Execute(boardId);
          }
      }
    });

    ChangeSortFieldCommand = new((key) =>
    {
      if (Enum.TryParse<NoteSortField>(key, out var sortField))
      {
        SortField = sortField;
        SortNoteViewModels();
        _settingsService.SetBoardSettings(AppSettingsKeys.BoardNoteSortField, (int)sortField);
      }
    });
    ChangeSortDirectionCommand = new((direction) =>
    {
      if (Enum.TryParse<SortDirection>(direction, out var sortDirection))
      {
        SortDirection = sortDirection;
        SortNoteViewModels();
        _settingsService.SetBoardSettings(AppSettingsKeys.BoardNoteSortDirection, (int)sortDirection);
      }
    });

    ShowRenameBoardDialogCommand = new(async () =>
    {
      if (Navigation is NavigationUserBoard userBoard)
      {
        var result = await _dialogService.ShowRenameBoardDialog(userBoard);
        if (result.DialogResult)
        {
          if (result.Icon is not null)
            Navigation.Icon = result.Icon;
          if (result.Name is not null)
            Navigation.Name = result.Name;
        }
      }
    });

    ShowDeleteBoardDialogCommand = new(async () =>
    {
      if (Navigation is NavigationUserBoard userBoard)
        await _dialogService.ShowDeleteBoardDialog(userBoard);
    });

    ChangeIconCommand = new((icon) => Navigation.Icon = icon);
  }
  #endregion

  #region Messengers
  private void RegisterMessengers()
  {
    WeakReferenceMessenger.Default.Register<Message<NoteViewModel>, string>(this, Tokens.RemoveNoteFromBookmarks, new((recipient, message) =>
    {
      var noteViewModel = message.Content;
      if (Navigation is NavigationBookmarks)
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