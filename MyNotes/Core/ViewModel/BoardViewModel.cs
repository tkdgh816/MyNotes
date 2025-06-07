using MyNotes.Common.Collections;
using MyNotes.Common.Commands;
using MyNotes.Common.Messaging;
using MyNotes.Core.Model;
using MyNotes.Core.Service;

namespace MyNotes.Core.ViewModel;

internal class BoardViewModel : ViewModelBase
{
  public BoardViewModel(NavigationBoard navigation, WindowService windowService, NoteService noteService)
  {
    Navigation = navigation;
    _windowService = windowService;
    _noteService = noteService;

    NoteViewModels = noteService.GetNoteViewModels(navigation);
    NoteViewModels.SortDescriptions.Add(new SortDescription<NoteViewModel>(func: vm => vm.Note.Modified, direction: SortDirection.Descending, keyPropertyName: "Modified"));
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
      App.Current.GetService<BoardViewModelFactory>().Close(Navigation);
    }
    NoteViewModels.Clear();
    base.Dispose(disposing);
  }

  private readonly WindowService _windowService;
  private readonly NoteService _noteService;

  public NavigationBoard Navigation { get; }

  #region NoteViewModels: Notes Collection
  public SortedObservableCollection<NoteViewModel> NoteViewModels { get; } = null!;

  #endregion

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

    WeakReferenceMessenger.Default.Send(new Message(this, NoteViewModels), Tokens.RefreshSource);
  }
  #endregion

  #region Commands
  public Command? AddNewNoteCommand { get; private set; }
  public Command<string>? SearchNotesCommand { get; private set; }
  public Command<string>? ChangeSortKeyCommand { get; private set; }
  public Command<string>? ChangeSortDirectionCommand { get; private set; }
  public Command? ShowRenameBoardDialogCommand { get; private set; }
  public Command? ShowRemoveBoardDialogCommand { get; private set; }
  public Command<Icon>? ChangeIconCommand { get; private set; }

  private void SetCommands()
  {
    AddNewNoteCommand = new(() =>
    {
      var newNote = _noteService.CreateNote((NavigationUserBoard)Navigation);
      newNote.NoteViewModel.CreateWindow();
    });

    SearchNotesCommand = new((query) =>
    {
      if (string.IsNullOrEmpty(query))
      {
        WeakReferenceMessenger.Default.Send(new Message(this, NoteViewModels), Tokens.ChangeSourceUnfiltered);
      }
      else
      {
        var source = NoteViewModels.Where(vm => vm.Note.Title.Contains(query) || vm.Note.Body.Contains(query));
        WeakReferenceMessenger.Default.Send(new Message(this, source), Tokens.ChangeSourceFiltered);
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

    ShowRenameBoardDialogCommand = new(() => WeakReferenceMessenger.Default.Send(new Message(Navigation), Tokens.RenameBoardName));
    ShowRemoveBoardDialogCommand = new(() => WeakReferenceMessenger.Default.Send(new Message(Navigation), Tokens.RemoveBoard));

    ChangeIconCommand = new((icon) => Navigation.Icon = icon);
  }
  #endregion

  #region Messengers
  private void RegisterMessengers()
  {
    WeakReferenceMessenger.Default.Register<Message, string>(this, Tokens.RemoveUnbookmarkedNote, new((recipient, message) =>
    {
      var noteViewModel = (NoteViewModel)message.Sender!;
      if (Navigation is NavigationBookmarks)
        NoteViewModels.Remove(noteViewModel);
    }));
  }

  private void UnregisterMessengers()
  {
    WeakReferenceMessenger.Default.UnregisterAll(this);
  }
  #endregion
}