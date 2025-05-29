using MyNotes.Common.Collections;
using MyNotes.Common.Commands;
using MyNotes.Core.Models;
using MyNotes.Core.Services;

namespace MyNotes.Core.ViewModels;

public class NoteHubViewModel : ViewModelBase
{
  public NoteHubViewModel(DatabaseService databaseService, NavigationService navigationService)
  {
    DatabaseService = databaseService;
    NavigationService = navigationService;
    Navigation = (NavigationNotes)NavigationService.CurrentNavigation!;

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
    NotesCollectionViewSource.Source = NoteViewModels;
    SetCommands();
  }

  static NoteViewModel InitializeNoteViewModel(Note note)
  {
    note.Initialize();
    NoteViewModel noteViewModel = ((App)Application.Current).GetService<NoteViewModelFactory>().Create(note);
    return noteViewModel;
  }

  public DatabaseService DatabaseService { get; }
  public NavigationService NavigationService { get; }
  public CollectionViewSource NotesCollectionViewSource { get; } = new();

  public NavigationNotes Navigation { get; }
  public SortedObervableCollection<NoteViewModel> NoteViewModels { get; } = new();

  public void CreateNewNote()
  {
    DateTimeOffset creationTime = DateTimeOffset.UtcNow;
    Note newNote = new(Guid.NewGuid(), ((NavigationBoard)Navigation).Id, "New note", creationTime, creationTime) { Background = Colors.LightGoldenrodYellow };
    NoteViewModel noteViewModel = InitializeNoteViewModel(newNote);
    NoteViewModels.Add(noteViewModel);
    DatabaseService.AddNote(newNote);
    noteViewModel.CreateWindow();
  }

  // Commands
  public Command? CreateNoteCommand { get; private set; }
  public Command<Note>? RemoveNoteCommand { get; private set; }
  public Command<string>? SortCommand { get; private set; }

  private void SetCommands()
  {
    CreateNoteCommand = new(CreateNewNote);

    SortCommand = new((sortKey) =>
    {
      SortDescription<NoteViewModel>? sortDescription;
      sortDescription = sortKey switch
      {
        "Modified" => new SortDescription<NoteViewModel>(func: vm => vm.Note.Modified, direction: SortDirection.Descending, keyPropertyName: "Modified"),
        "Created" => new SortDescription<NoteViewModel>(func: vm => vm.Note.Created, direction: SortDirection.Descending, keyPropertyName: "Created"),
        "Title" => new SortDescription<NoteViewModel>(func: vm => vm.Note.Title, direction: SortDirection.Ascending, keyPropertyName: "Title"),
        _ => null
      };
      if (sortDescription is not null)
      {
        NoteViewModels.SortDescriptions.Clear();
        NoteViewModels.SortDescriptions.Add(sortDescription);
        NoteViewModels.Refresh();
      }
      NotesCollectionViewSource.Source = NoteViewModels;
    });
  }
}
