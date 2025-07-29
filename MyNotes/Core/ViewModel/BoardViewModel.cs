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
  private readonly WindowService _windowService;
  private readonly DialogService _dialogService;
  private readonly NoteService _noteService;
  private readonly NoteViewModelFactory _noteViewModelFactory;

  public IncrementalObservableCollection<NoteViewModel> NoteViewModels { get; private set; } = null!;
  //public ObservableCollection<NoteViewModel> NoteViewModels { get; private set; } = new();
  public List<Note> Notes { get; private set; } = new();
  private bool _isCompleted = false;

  public BoardViewModel(NavigationBoard navigation, WindowService windowService, DialogService dialogService, NoteService noteService, NoteViewModelFactory noteViewModelFactory)
  {
    Navigation = navigation;
    _windowService = windowService;
    _dialogService = dialogService;
    _noteService = noteService;
    _noteViewModelFactory = noteViewModelFactory;

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

  private int _loadedNoteCount = 0;

  private void GetNoteViewModels()
  {
    //GetNoteViewModelsBatch0();
    //GetNoteViewModelsStream0();

    if (Navigation is NavigationUserBoard)
    {
      Task.Run(async () =>
      {
        await foreach (var note in _noteService.GetNotesStreamAsync(Navigation))
          Notes.Add(note);
        _isCompleted = true;
      });
    }
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
        uint index = 0;
        while (index < count)
        {
          if (!_isCompleted)
            await Task.Delay(100);
          else if (_loadedNoteCount >= Notes.Count)
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
  public NoteSortKey SortKey { get; private set; }
  public SortDirection SortDirection { get; private set; }

  private void SortNoteViewModels()
  {
    WeakReferenceMessenger.Default.Send(new Message<IEnumerable<NoteViewModel>>(NoteViewModels, this), Tokens.RefreshSource);
  }
  #endregion

  #region Commands
  public Command? AddNewNoteCommand { get; private set; }
  public Command<string>? SearchNotesCommand { get; private set; }
  public Command<string>? ChangeSortKeyCommand { get; private set; }
  public Command<string>? ChangeSortDirectionCommand { get; private set; }
  public Command? ShowRenameBoardDialogCommand { get; private set; }
  public Command? ShowDeleteBoardDialogCommand { get; private set; }
  public Command<Icon>? ChangeIconCommand { get; private set; }

  private void SetCommands()
  {
    AddNewNoteCommand = new(() =>
    {
      Note newNote = _noteService.CreateNote((NavigationUserBoard)Navigation);
      NoteViewModel noteViewModel = _noteViewModelFactory.Resolve(newNote);
      NoteViewModels.Insert(0, noteViewModel);
      noteViewModel.CreateWindow();
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

    ChangeSortKeyCommand = new((key) =>
    {
      if (Enum.TryParse<NoteSortKey>(key, out var sortKey))
      {
        SortKey = sortKey;
        SortNoteViewModels();
      }
    });
    ChangeSortDirectionCommand = new((direction) =>
    {
      if (Enum.TryParse<SortDirection>(direction, out var sortDirection))
      {
        SortDirection = sortDirection;
        SortNoteViewModels();
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