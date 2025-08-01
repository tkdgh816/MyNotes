using CommunityToolkit.WinUI;

using Microsoft.UI.Dispatching;

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
  //public ObservableCollection<NoteViewModel> NoteViewModels { get; private set; } = new();
  public SortedCollection<Note> Notes { get; private set; } = null!;
  private bool _isCompleted = false;

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

      NoteViewModels.Clear();
      UnregisterMessengers();
      App.Instance.GetService<BoardViewModelFactory>().Remove(Navigation);
    }

    base.Dispose(disposing);
  }

  public NoteSortField SortField { get; private set; } = NoteSortField.Created;
  public SortDirection SortDirection { get; private set; } = SortDirection.Ascending;

  public void InitializeSettings()
  {
    var settings = _settingsService.GetBoardSettings();
    SortField = settings.SortField;
    SortDirection = settings.SortDirection;
  }

  private int _loadedNoteCount = 0;

  private void GetNotes()
  {
    Notes = new(GetNoteComparer());
    if (Navigation is NavigationUserBoard)
    {
      Task.Run(async () =>
      {
        await foreach (var note in _noteService.GetNotesStreamAsync(Navigation, sortField: SortField, sortDirection: SortDirection))
          Notes.Add(note);
        _isCompleted = true;
      });
    }
  }

  private void GetNoteViewModels()
  {
    //GetNoteViewModelsBatch0();
    //GetNoteViewModelsStream0();
    NoteViewModels = new(GetNoteViewModelsBatch1);
    //NoteViewModels = new(GetNoteViewModelsBatch2);
    //NoteViewModels = new(GetNoteViewModelsStream1);
  }

  private async void GetNoteViewModelsBatch0()
  {
    var notes = Navigation switch
    {
      NavigationSearch search => await _noteService.SearchNotesBatchAsync(search.SearchText),
      _ => await _noteService.GetNotesBatchAsync(Navigation)
    };

    foreach (var vm in notes.Select(_noteViewModelFactory.Resolve))
      NoteViewModels.Add(vm);
  }

  private async void GetNoteViewModelsStream0()
  {
    var notes = Navigation switch
    {
      NavigationSearch search => _noteService.SearchNotesStreamAsync(search.SearchText),
      _ => _noteService.GetNotesStreamAsync(Navigation),
    };

    await foreach (var note in notes)
      NoteViewModels.Add(_noteViewModelFactory.Resolve(note));
  }

  private async Task<IEnumerable<NoteViewModel>> GetNoteViewModelsBatch1(uint count)
  {
    List<NoteViewModel> vms = new();
    switch (Navigation)
    {
      case NavigationSearch search:
        var notes = await _noteService.SearchNotesBatchAsync(search.SearchText, (int)count, _loadedNoteCount);
        vms = [.. notes.Select(_noteViewModelFactory.Resolve)];
        _loadedNoteCount += vms.Count;
        break;
      default:
        if (!_isCompleted)
          await Task.Delay(100);

        uint index = 0;
        while (index < count)
        {
          if (_loadedNoteCount >= Notes.Count)
            break;
          else
          {
            vms.Add(_noteViewModelFactory.Resolve(Notes[_loadedNoteCount++]));
            index++;
          }
        }
        break;
    }
    return vms;
  }

  private async Task<IEnumerable<NoteViewModel>> GetNoteViewModelsBatch2(uint count)
  {
    var notes = Navigation switch
    {
      NavigationSearch search => await _noteService.SearchNotesBatchAsync(search.SearchText, (int)count, NoteViewModels.Count),
      _ => await _noteService.GetNotesBatchAsync(Navigation, (int)count, NoteViewModels.Count)
    };
    return notes.Select(_noteViewModelFactory.Resolve);
  }

  private async IAsyncEnumerable<NoteViewModel> GetNoteViewModelsStream1(uint count)
  {
    var notes = Navigation switch
    {
      NavigationSearch search => _noteService.SearchNotesStreamAsync(search.SearchText, (int)count, NoteViewModels.Count),
      _ => _noteService.GetNotesStreamAsync(Navigation, (int)count, NoteViewModels.Count),
    };

    await foreach (var note in notes)
      yield return _noteViewModelFactory.Resolve(note);
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
    _loadedNoteCount = 0;
    NoteViewModels.Clear();
  }
  #endregion

  #region Commands
  public Command? AddNewNoteCommand { get; private set; }
  public Command<string>? SearchNotesCommand { get; private set; }
  public Command<string>? ChangeSortFieldCommand { get; private set; }
  public Command<string>? ChangeSortDirectionCommand { get; private set; }
  public Command? ShowRenameBoardDialogCommand { get; private set; }
  public Command? ShowDeleteBoardDialogCommand { get; private set; }
  public Command<Icon>? ChangeIconCommand { get; private set; }

  private void SetCommands()
  {
    AddNewNoteCommand = new(() =>
    {
      for (int i = 0; i < 20; i++)
      {
        Note newNote = _noteService.CreateNote((NavigationUserBoard)Navigation);
        NoteViewModel noteViewModel = _noteViewModelFactory.Resolve(newNote);
        NoteViewModels.Insert(0, noteViewModel);
        noteViewModel.CreateWindow();
        DispatcherQueue.GetForCurrentThread().EnqueueAsync(async () =>
        {
          await Task.Delay(2000);
          noteViewModel.CloseWindowCommand?.Execute();
        });
      }
    });

    SearchNotesCommand = new((query) =>
    {
      if (string.IsNullOrEmpty(query))
      {
        WeakReferenceMessenger.Default.Send(new Message<IEnumerable<NoteViewModel>>(NoteViewModels, this), Tokens.ChangeSourceUnfiltered);
      }
      else
      {
        var source = NoteViewModels.Where(vm => vm.Note.Title.Contains(query) || vm.Note.Preview.Contains(query));
        WeakReferenceMessenger.Default.Send(new Message<IEnumerable<NoteViewModel>>(source, this), Tokens.ChangeSourceFiltered);
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