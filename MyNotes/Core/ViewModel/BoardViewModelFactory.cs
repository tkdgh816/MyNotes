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

    var windowService = App.Current.GetService<WindowService>();
    var dialogService = App.Current.GetService<DialogService>();
    var noteService = App.Current.GetService<NoteService>();
    var noteViewModelFactory = App.Current.GetService<NoteViewModelFactory>();

    BoardViewModel newViewModel = new(navigation, windowService, dialogService, noteService, noteViewModelFactory);
    _cache.Remove(navigation);
    _cache.Add(navigation, new(newViewModel));
    ReferenceTracker.BoardViewModelReferences.Add(new(newViewModel));
    return newViewModel;
  }
}
