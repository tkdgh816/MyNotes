using MyNotes.Models;

using Windows.ApplicationModel.Resources;

namespace MyNotes.ViewModels;

public partial class MainViewModel : ViewModelBase
{
  public ObservableCollection<INavigation> MenuItems { get; } = new();
  public ObservableCollection<INavigation> FooterMenuItems { get; } = new();

  public MainViewModel()
  {
    var home = NavigationHome.Instance;
    Debug.WriteLine(home.Title);

    MenuItems.Add(NavigationHome.Instance);
    MenuItems.Add(NavigationBookmarks.Instance);
    MenuItems.Add(new NavigationSeparator());

    FooterMenuItems.Add(NavigationTrash.Instance);
    FooterMenuItems.Add(NavigationSettings.Instance);
  }
}