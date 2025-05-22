using MyNotes.Core.Models;
using MyNotes.Core.Services;
using MyNotes.Core.Views;

namespace MyNotes.Core.ViewModels;

public class NoteBoardViewModel : ViewModelBase
{
  public NoteBoardViewModel(WindowService windowService, DatabaseService databaseService, NavigationService navigationService)
  {
    WindowService = windowService;
    DatabaseService = databaseService;
    NavigationService = navigationService;
    Navigation = (NavigationBoardItem)NavigationService.CurrentNavigation!;
    Navigation.Notes.Clear();
    foreach (Note note in DatabaseService.GetNotes(Navigation))
    {
      note.Initialize();
      Navigation.Notes.Add(note);
    }
    NotesCollectionViewSource.Source = Navigation.Notes;
  }

  public WindowService WindowService { get; }
  public DatabaseService DatabaseService { get; }
  public NavigationService NavigationService { get; }
  public CollectionViewSource NotesCollectionViewSource { get; } = new();

  public NavigationBoardItem Navigation { get; }

  public void CreateNewNote()
  {
    DateTimeOffset creationTime = DateTimeOffset.UtcNow;
    Note newNote = new(Guid.NewGuid(), Navigation, "New note", creationTime, creationTime) { Background = Colors.LightGoldenrodYellow };
    newNote.Initialize();
    Navigation.Notes.Add(newNote);
    DatabaseService.AddNote(newNote);
    CreateNoteWindow(newNote);
  }

  public void CreateNoteWindow(Note note)
  {
    NoteWindow window = WindowService.CreateNoteWindow(note);
    window.Activate();
    //WindowService.ActivateNoteWindow(note);
  }
}
