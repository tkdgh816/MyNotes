using MyNotes.Core.Model;
using MyNotes.Core.Service;
using MyNotes.Debugging;

namespace MyNotes.Core.ViewModel;

internal class NoteViewModelFactory : ViewModelFactoryBase<Note, NoteViewModel>
{
  public override NoteViewModel Create(Note note)
  {
    if (_cache.TryGetValue(note, out WeakReference<NoteViewModel>? wr))
      if (wr.TryGetTarget(out NoteViewModel? noteViewModel))
        return noteViewModel;

    WindowService windowService = App.Current.GetService<WindowService>();
    NavigationService navigationService = App.Current.GetService<NavigationService>();
    DatabaseService databaseService = App.Current.GetService<DatabaseService>();
    NoteService noteService = App.Current.GetService<NoteService>();
    NoteViewModel newViewModel = new(note, windowService, navigationService, databaseService, noteService);
    _cache.Remove(note);
    _cache.Add(note, new(newViewModel));
    ReferenceTracker.NoteViewModelReferences.Add(new(newViewModel));
    return newViewModel;
  }
}
