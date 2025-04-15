using MyNotes.Core.Models;
using MyNotes.Core.Services;

namespace MyNotes.Core.ViewModels;

public class MainViewModel : ViewModelBase
{
  public MainViewModel(DatabaseService databaseService, NavigationService navigationService)
  {
    DatabaseService = databaseService;
    NavigationService = navigationService;

    NavigationGroupItem group1 = new("group 1", new BitmapIconSource() { UriSource = new Uri("ms-appx:///Assets/icons/emojis/0.png"), ShowAsMonochrome = false }, new Guid());
    group1.Children.Add(new NavigationBoardItem("Board 1", new BitmapIconSource() { UriSource = new Uri("ms-appx:///Assets/icons/emojis/0.png"), ShowAsMonochrome = false }, new Guid()));
    group1.Children.Add(new NavigationBoardItem("Board 2", new BitmapIconSource() { UriSource = new Uri("ms-appx:///Assets/icons/emojis/0.png"), ShowAsMonochrome = false }, new Guid()));
    group1.Children.Add(new NavigationBoardItem("Board 3", new FontIconSource() { Glyph = "\uE70B" }, new Guid()));

    NavigationGroupItem group2 = new("group 2", new FontIconSource() { Glyph = "\uED41" }, new Guid());
    group2.Children.Add(new NavigationBoardItem("Board 1", new FontIconSource() { Glyph = "\uE70B" }, new Guid()));
    group2.Children.Add(new NavigationBoardItem("Board 2", new FontIconSource() { Glyph = "\uE70B" }, new Guid()));
    group2.Children.Add(new NavigationBoardItem("Board 3", new FontIconSource() { Glyph = "\uE70B" }, new Guid()));

    NavigationGroupItem group3 = new("group 3", new FontIconSource() { Glyph = "\uED41" }, new Guid());
    group3.Children.Add(new NavigationBoardItem("Board 1", new FontIconSource() { Glyph = "\uE70B" }, new Guid()));
    group3.Children.Add(new NavigationBoardItem("Board 2", new FontIconSource() { Glyph = "\uE70B" }, new Guid()));
    group3.Children.Add(new NavigationBoardItem("Board 3", new FontIconSource() { Glyph = "\uE70B" }, new Guid()));
    group1.Children.Add(group3);

    ListMenuItems.Add(group1);
    ListMenuItems.Add(group2);

    MenuItems.Source = new List<IList>() { CoreMenuItems, ListMenuItems };
  }

  public DatabaseService DatabaseService { get; }
  public NavigationService NavigationService { get; }
  public CollectionViewSource MenuItems { get; } = new() { IsSourceGrouped = true };

  public List<Navigation> CoreMenuItems { get; } =
  [
    new NavigationHomeItem(),
    new NavigationBookmarksItem(),
    new NavigationTagsItem(),
    new NavigationSeparator(),
  ];
  public ObservableCollection<NavigationItem> ListMenuItems { get; } = new();
  public List<Navigation> FooterMenuItems { get; } =
  [
    new NavigationTrashItem(),
    new NavigationSettingsItem()
  ];

  public void Recursive(NavigationItem item, Action<NavigationItem> action)
  {
    if (item is NavigationGroupItem group)
      foreach (NavigationItem child in group.Children)
        Recursive(child, action);
    action.Invoke(item);
  }
}