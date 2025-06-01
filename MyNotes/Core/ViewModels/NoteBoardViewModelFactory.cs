using MyNotes.Core.Models;
using MyNotes.Core.Services;

namespace MyNotes.Core.ViewModels;

internal class NoteBoardViewModelFactory : ViewModelFactoryBase<NavigationNotes, NoteBoardViewModel>
{
  public override NoteBoardViewModel Create(NavigationNotes navigation)
  {
    if (_cache.TryGetValue(navigation, out WeakReference<NoteBoardViewModel>? wr))
      if (wr.TryGetTarget(out NoteBoardViewModel? viewModel))
        return viewModel;

    var databaseService = App.Current.GetService<DatabaseService>();
    NoteBoardViewModel newViewModel = new NoteBoardViewModel(navigation, databaseService);
    _cache.Remove(navigation);
    _cache.Add(navigation, new(newViewModel));
    return newViewModel;
  }
}
