using MyNotes.Common.Collections;
using MyNotes.Common.Commands;
using MyNotes.Common.Messaging;
using MyNotes.Core.Models;
using MyNotes.Core.Services;

namespace MyNotes.Core.ViewModels;

public class NoteBoardViewModel : ViewModelBase
{
  public NoteBoardViewModel(NavigationNotes navigation, DatabaseService databaseService)
  {
    DatabaseService = databaseService;
    Navigation = navigation;

    if (Navigation is NavigationBoard navigationBoard)
    {
      foreach (Note note in DatabaseService.GetNotes(navigationBoard))
        NoteViewModels.Add(InitializeNoteViewModel(note));
    }
    else if (Navigation is NavigationBookmarks navigationBookmarks)
    {
      foreach (Note note in DatabaseService.GetBookmarkedNotes())
        NoteViewModels.Add(InitializeNoteViewModel(note));
    }
    else if (Navigation is NavigationTrash navigationTrash)
    {
      foreach (Note note in DatabaseService.GetTrashedNotes())
        NoteViewModels.Add(InitializeNoteViewModel(note));
    }

    RegisterEvents();
    SetCommands();
    RegisterMessengers();
  }

  public override void Dispose()
  {
    UnregisterEvents();
    NoteViewModels.Clear();
    //App.Current.GetService<NoteBoardViewModelFactory>().Dispose(Navigation);
    GC.SuppressFinalize(this);
  }

  private readonly DatabaseService DatabaseService;

  #region Handling Events
  private void RegisterEvents()
  {
    Navigation.PropertyChanged += OnNavigationPropertyChanged;
  }

  private void UnregisterEvents()
  {
    Navigation.PropertyChanged -= OnNavigationPropertyChanged;
  }
  #endregion

  #region Navigation
  public NavigationNotes Navigation { get; }
  private void OnNavigationPropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (e.PropertyName == "Icon")
      DatabaseService.UpdateBoard((NavigationBoard)Navigation, "Icon");
  }
  #endregion

  #region NoteViewModels: Notes Collection
  public SortedObervableCollection<NoteViewModel> NoteViewModels { get; } = new([new SortDescription<NoteViewModel>(func: vm => vm.Note.Modified, direction: SortDirection.Descending, keyPropertyName: "Modified")]);

  static NoteViewModel InitializeNoteViewModel(Note note)
  {
    note.Initialize();
    NoteViewModel noteViewModel = App.Current.GetService<NoteViewModelFactory>().Create(note);
    return noteViewModel;
  }
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

  private void SetCommands()
  {
    AddNewNoteCommand = new(() =>
    {
      DateTimeOffset creationTime = DateTimeOffset.UtcNow;
      Note newNote = new(Guid.NewGuid(), ((NavigationBoard)Navigation).Id, "New note", creationTime, creationTime) { Background = Colors.LightGoldenrodYellow };
      NoteViewModel noteViewModel = InitializeNoteViewModel(newNote);
      NoteViewModels.Add(noteViewModel);
      DatabaseService.AddNote(newNote);
      noteViewModel.CreateWindow();
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
  #endregion
}