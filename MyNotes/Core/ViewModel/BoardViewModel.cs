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

  public SortedObservableCollection<NoteViewModel> NoteViewModels { get; private set; } = null!;

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
      UnregisterMessengers();
      App.Instance.GetService<BoardViewModelFactory>().Remove(Navigation);
    }
    NoteViewModels.Clear();
    base.Dispose(disposing);
  }

  private void GetNoteViewModels()
  {
    NoteViewModels = Navigation switch
    {
      NavigationUserBoard userBoard => new(_noteService.GetNotes(userBoard).Select(_noteViewModelFactory.Resolve)),
      NavigationBookmarks => new(_noteService.GetBookmarkedNotes().Select(_noteViewModelFactory.Resolve)),
      NavigationTrash => new(_noteService.GetTrashedNotes().Select(_noteViewModelFactory.Resolve)),
      _ => new(),
    };

    NoteViewModels.SortDescriptions.Add(new SortDescription<NoteViewModel>(func: vm => vm.Note.Modified, direction: SortDirection.Descending, keyPropertyName: "Modified"));
  }

  #region Sort
  public NoteSortKey SortKey { get; private set; }
  public SortDirection SortDirection { get; private set; }

  private void SortNoteViewModels()
  {
    SortDescription<NoteViewModel> sortDescription = SortKey switch
    {
      NoteSortKey.Created => new(func: vm => vm.Note.Created, direction: SortDirection),
      NoteSortKey.Title => new(func: vm => vm.Note.Title, direction: SortDirection, keyPropertyName: "Title"),
      NoteSortKey.Modified or _ => new(func: vm => vm.Note.Modified, direction: SortDirection, keyPropertyName: "Modified"),
    };

    NoteViewModels.SortDescriptions.Clear();
    NoteViewModels.SortDescriptions.Add(sortDescription);
    NoteViewModels.Refresh();

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
      NoteViewModels.Add(noteViewModel);
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
        var source = NoteViewModels.Where(vm => vm.Note.Title.Contains(query) || vm.Note.Body.Contains(query));
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