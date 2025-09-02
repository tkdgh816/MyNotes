using MyNotes.Core.Model;
using MyNotes.Core.Service;
using MyNotes.Debugging;

namespace MyNotes.Core.ViewModel;

internal class BoardViewModelFactory(SettingsService settingsService, WindowService windowService, DialogService dialogService, NoteService noteService, NoteViewModelFactory noteViewModelFactory) : ViewModelFactoryBase<NavigationBoard, BoardViewModel>
{
  public override BoardViewModel Resolve(NavigationBoard navigation)
  {
    BoardViewModel? viewModel = Get(navigation);

    if (viewModel is not null)
      return viewModel;

    BoardViewModel newViewModel = new(navigation, settingsService, windowService, dialogService, noteService, noteViewModelFactory);
    _cache.Remove(navigation);
    _cache.Add(navigation, new(newViewModel));
    ReferenceTracker.BoardViewModelReferences.Add(new(navigation.Name, newViewModel));
    return newViewModel;
  }
}
