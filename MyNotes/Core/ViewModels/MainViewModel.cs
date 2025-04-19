using MyNotes.Core.Models;
using MyNotes.Core.Services;

namespace MyNotes.Core.ViewModels;

public class MainViewModel : ViewModelBase
{
  public MainViewModel(DatabaseService databaseService, NavigationService navigationService)
  {
    DatabaseService = databaseService;
    NavigationService = navigationService;

    NavigationGroupItem root = new("Root", new FontIconSource() { Glyph = "\uE82D" }, new Guid());
    NavigationGroupItem group1 = new("Group 1", new FontIconSource() { Glyph = "\uE82D" }, new Guid());
    group1.Children.Add(new NavigationBoardItem("Board 1", new FontIconSource() { Glyph = "\uE737" }, new Guid()));
    group1.Children.Add(new NavigationBoardItem("Board 2", new FontIconSource() { Glyph = "\uE737" }, new Guid()));
    group1.Children.Add(new NavigationBoardItem("Board 3", new FontIconSource() { Glyph = "\uE737" }, new Guid()));

    NavigationGroupItem group2 = new("Group 2", new FontIconSource() { Glyph = "\uE82D" }, new Guid());
    group2.Children.Add(new NavigationBoardItem("Board 1", new FontIconSource() { Glyph = "\uE737" }, new Guid()));
    group2.Children.Add(new NavigationBoardItem("Board 2", new FontIconSource() { Glyph = "\uE737" }, new Guid()));
    group2.Children.Add(new NavigationBoardItem("Board 3", new FontIconSource() { Glyph = "\uE737" }, new Guid()));

    NavigationGroupItem group3 = new("Group 3", new FontIconSource() { Glyph = "\uE82D" }, new Guid());
    group3.Children.Add(new NavigationBoardItem("Board 1", new FontIconSource() { Glyph = "\uE737" }, new Guid()));
    group3.Children.Add(new NavigationBoardItem("Board 2", new FontIconSource() { Glyph = "\uE737" }, new Guid()));
    group3.Children.Add(new NavigationBoardItem("Board 3", new FontIconSource() { Glyph = "\uE737" }, new Guid()));
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