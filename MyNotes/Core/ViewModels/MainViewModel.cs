using MyNotes.Core.Services;

namespace MyNotes.Core.ViewModels;

public class MainViewModel
{
  public MainViewModel(NavigationService navigationService)
  {
    NavigationService = navigationService;
  }

  public NavigationService NavigationService { get; }
}
