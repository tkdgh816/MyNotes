using System.Runtime.CompilerServices;

using MyNotes.Core.Models;
using MyNotes.Core.Services;

namespace MyNotes.Core.ViewModels;

internal class NoteViewModelFactory
{
  public NoteViewModelFactory() { }

  private readonly ConditionalWeakTable<Note, WeakReference<NoteViewModel>> _cache = new();

  public NoteViewModel Create(Note note)
  {
    if (_cache.TryGetValue(note, out WeakReference<NoteViewModel>? wr))
      if (wr.TryGetTarget(out NoteViewModel? noteViewModel))
        return noteViewModel;

    var windowService = App.Current.GetService<WindowService>();
    var databaseService = App.Current.GetService<DatabaseService>();
    NoteViewModel newViewModel = new NoteViewModel(note, windowService, databaseService);
    _cache.Remove(note);
    _cache.Add(note, new(newViewModel));
    return newViewModel;
  }
}
