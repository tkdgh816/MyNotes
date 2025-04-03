using MyNotes.Core.Models;
using MyNotes.Core.Services;

namespace MyNotes.Core.ViewModels;

public class MainViewModel : ViewModelBase
{
  public MainViewModel(NavigationService navigationService)
  {
    NavigationService = navigationService;

    List<Navigation> CoreMenuItemsGroup =
    [
      new NavigationHomeItem(),
      new NavigationBookmarksItem(),
      new NavigationTagsItem(),
      new NavigationSeparator(),
    ];
    ObservableCollection<Navigation> ListMenuItemsGroup = new();
    FooterMenuItems.Add(new NavigationTrashItem());
    FooterMenuItems.Add(new NavigationSettingsItem());
    MenuItems.Source = new List<IList>() { CoreMenuItemsGroup, ListMenuItemsGroup };
  }

  public NavigationService NavigationService { get; }
  public CollectionViewSource MenuItems { get; } = new() { IsSourceGrouped = true };
  public List<Navigation> FooterMenuItems { get; } = new();
  public Stack<NavigationItem> PreviousNavigations { get; } = new();
  public NavigationItem? CurrentNavigation { get; set; }

  private bool _isBackEnabled = false;
  public bool IsBackEnabled
  {
    get => _isBackEnabled;
    set => SetProperty(ref _isBackEnabled, value);
  }
  public bool IsBackRequested = false;

}
