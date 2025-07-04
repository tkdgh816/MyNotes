using MyNotes.Core.Model;
using MyNotes.Core.Service;
using MyNotes.Debugging;

namespace MyNotes.Core.ViewModel;

internal class NoteViewModelFactory(WindowService windowService, DialogService dialogService, NoteService noteService, TagService tagService) : ViewModelFactoryBase<Note, NoteViewModel>
{
  public override NoteViewModel Resolve(Note note)
  {
    NoteViewModel? viewModel = Get(note);

    if (viewModel is not null)
      return viewModel;

    NoteViewModel newViewModel = new(note, windowService, dialogService, noteService, tagService);
    _cache.Remove(note);
    _cache.Add(note, new(newViewModel));
    ReferenceTracker.NoteViewModelReferences.Add(new(note.Title, newViewModel));
    return newViewModel;
  }
}
