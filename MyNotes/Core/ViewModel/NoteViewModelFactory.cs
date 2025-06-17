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

    var windowService = App.Current.GetService<WindowService>();
    var dialogService = App.Current.GetService<DialogService>();
    var noteService = App.Current.GetService<NoteService>();
    var tagService = App.Current.GetService<TagService>();

    NoteViewModel newViewModel = new(note, windowService, dialogService, noteService, tagService);
    _cache.Remove(note);
    _cache.Add(note, new(newViewModel));
    ReferenceTracker.NoteViewModelReferences.Add(new(newViewModel));
    return newViewModel;
  }
}
