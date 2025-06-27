using MyNotes.Core.Model;
using MyNotes.Core.Service;
using MyNotes.Debugging;

namespace MyNotes.Core.ViewModel;

internal class BoardViewModelFactory : ViewModelFactoryBase<NavigationBoard, BoardViewModel>
{
  public override BoardViewModel Resolve(NavigationBoard navigation)
  {
    if (_cache.TryGetValue(navigation, out WeakReference<BoardViewModel>? wr))
      if (wr.TryGetTarget(out BoardViewModel? viewModel))
        return viewModel;

    var windowService = App.Instance.GetService<WindowService>();
    var dialogService = App.Instance.GetService<DialogService>();
    var noteService = App.Instance.GetService<NoteService>();
    var noteViewModelFactory = App.Instance.GetService<NoteViewModelFactory>();

    BoardViewModel newViewModel = new(navigation, windowService, dialogService, noteService, noteViewModelFactory);
    _cache.Remove(navigation);
    _cache.Add(navigation, new(newViewModel));
    ReferenceTracker.BoardViewModelReferences.Add(new(navigation.Name, newViewModel));
    return newViewModel;
  }
}
