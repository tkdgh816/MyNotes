using MyNotes.Common.Collections;
using MyNotes.Common.Commands;
using MyNotes.Core.Models;
using MyNotes.Core.Services;
using MyNotes.Core.Views;

namespace MyNotes.Core.ViewModels;

public class NoteHubViewModel : ViewModelBase
{
  public NoteHubViewModel(WindowService windowService, DatabaseService databaseService, NavigationService navigationService)
  {
    WindowService = windowService;
    DatabaseService = databaseService;
    NavigationService = navigationService;
    Navigation = (NavigationNotes)NavigationService.CurrentNavigation!;
    Navigation.Notes.Clear();
    if (Navigation is NavigationBoard navigationBoard)
    {
      foreach (Note note in DatabaseService.GetNotes(navigationBoard))
      {
        note.Initialize();
        Navigation.Notes.Add(note);
      }
    }
    else if (Navigation is NavigationBookmarks navigationBookmarks)
    {
      foreach (Note note in DatabaseService.GetBookmarkedNotes())
      {
        note.Initialize();
        Navigation.Notes.Add(note);
      }
    }
    NotesCollectionViewSource.Source = Navigation.Notes;
    SetCommands();
  }

  public WindowService WindowService { get; }
  public DatabaseService DatabaseService { get; }
  public NavigationService NavigationService { get; }
  public CollectionViewSource NotesCollectionViewSource { get; } = new();

  public NavigationNotes Navigation { get; }

  public void CreateNewNote()
  {
    DateTimeOffset creationTime = DateTimeOffset.UtcNow;
    Note newNote = new(Guid.NewGuid(), ((NavigationBoard)Navigation).Id, "New note", creationTime, creationTime) { Background = Colors.LightGoldenrodYellow };
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

  public void UpdateNote(Note note, string propertyName)
  {
    if (propertyName == nameof(Note.IsBookmarked) && Navigation is NavigationBookmarks && !note.IsBookmarked)
      Navigation.Notes.Remove(note);
    DatabaseService.UpdateNote(note, propertyName, false);
  }

  public void UpdateNoteTag(Note note, string tag)
  {
    if (string.IsNullOrWhiteSpace(tag) || note.Tags.Contains(tag))
      return;

    note.Tags.Add(tag);
    DatabaseService.AddTag(note, tag);
  }

  // Commands
  public Command<Note>? RemoveNoteCommand { get; private set; }
  public Command<string>? SortCommand { get; private set; }

  private void SetCommands()
  {
    RemoveNoteCommand = new((note) =>
    {
      note.IsTrashed = true;
      DatabaseService.UpdateNote(note, nameof(Note.IsTrashed), false);
      Navigation.Notes.Remove(note);
    });

    SortCommand = new((sortKey) =>
    {
      SortDescription<Note>? sortDescription;
      sortDescription = sortKey switch
      {
        "Modified" => new SortDescription<Note>(func: note => note.Modified, direction: SortDirection.Descending, keyPropertyName: "Modified"),
        "Created" => new SortDescription<Note>(func: note => note.Created, direction: SortDirection.Descending, keyPropertyName: "Created"),
        "Title" => new SortDescription<Note>(func: note => note.Title, direction: SortDirection.Ascending, keyPropertyName: "Title"),
        _ => null
      };
      if(sortDescription is not null)
      {
        Navigation.Notes.SortDescriptions.Clear();
        Navigation.Notes.SortDescriptions.Add(sortDescription);
        Navigation.Notes.Refresh();
      }
      NotesCollectionViewSource.Source = Navigation.Notes;
    });
  }
}
