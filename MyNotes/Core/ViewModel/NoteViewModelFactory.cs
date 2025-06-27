using MyNotes.Core.Model;
using MyNotes.Core.Service;
using MyNotes.Debugging;

namespace MyNotes.Core.ViewModel;

internal class NoteViewModelFactory : ViewModelFactoryBase<Note, NoteViewModel>
{
  public override NoteViewModel Resolve(Note note)
  {
    if (_cache.TryGetValue(note, out WeakReference<NoteViewModel>? wr))
      if (wr.TryGetTarget(out NoteViewModel? noteViewModel))
        return noteViewModel;

    var windowService = App.Instance.GetService<WindowService>();
    var dialogService = App.Instance.GetService<DialogService>();
    var noteService = App.Instance.GetService<NoteService>();
    var tagService = App.Instance.GetService<TagService>();

    NoteViewModel newViewModel = new(note, windowService, dialogService, noteService, tagService);
    _cache.Remove(note);
    _cache.Add(note, new(newViewModel));
    ReferenceTracker.NoteViewModelReferences.Add(new(note.Title, newViewModel));
    return newViewModel;
  }
}
