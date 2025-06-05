using MyNotes.Core.Model;
using MyNotes.Core.Service;
using MyNotes.Debugging;

namespace MyNotes.Core.ViewModel;

internal class BoardViewModelFactory : ViewModelFactoryBase<NavigationBoard, BoardViewModel>
{
  public override BoardViewModel Create(NavigationBoard navigation)
  {
    if (_cache.TryGetValue(navigation, out WeakReference<BoardViewModel>? wr))
      if (wr.TryGetTarget(out BoardViewModel? viewModel))
        return viewModel;

    WindowService windowService = App.Current.GetService<WindowService>();
    DatabaseService databaseService = App.Current.GetService<DatabaseService>();
    NoteService noteService = App.Current.GetService<NoteService>();
    BoardViewModel newViewModel = new(navigation, windowService, databaseService, noteService);
    _cache.Remove(navigation);
    _cache.Add(navigation, new(newViewModel));
    ReferenceTracker.BoardViewModelReferences.Add(new(newViewModel));
    return newViewModel;
  }
}
