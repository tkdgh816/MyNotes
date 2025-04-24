using MyNotes.Core.Models;
using MyNotes.Core.Services;

namespace MyNotes.Core.ViewModels;

public class MainViewModel : ViewModelBase
{
  public MainViewModel(DatabaseService databaseService, NavigationService navigationService)
  {
    DatabaseService = databaseService;
    NavigationService = navigationService;

    // TEST
    NavigationBoardGroupItem group1 = new("Group 1", new FontIconSource() { Glyph = "\uE82D" }, new Guid());
    group1.Add(new NavigationBoardItem("Group 1 Board 1", new FontIconSource() { Glyph = "\uE737" }, new Guid()));
    group1.Add(new NavigationBoardItem("Group 1 Board 2", new FontIconSource() { Glyph = "\uE737" }, new Guid()));
    group1.Add(new NavigationBoardItem("Group 1 Board 3", new FontIconSource() { Glyph = "\uE737" }, new Guid()));

    NavigationBoardGroupItem group2 = new("Group 2", new FontIconSource() { Glyph = "\uE82D" }, new Guid());
    group2.Add(new NavigationBoardItem("Group 2 Board 1", new FontIconSource() { Glyph = "\uE737" }, new Guid()));
    group2.Add(new NavigationBoardItem("Group 2 Board 2", new FontIconSource() { Glyph = "\uE737" }, new Guid()));
    group2.Add(new NavigationBoardItem("Group 2 Board 3", new FontIconSource() { Glyph = "\uE737" }, new Guid()));

    NavigationBoardGroupItem group3 = new("Group 3", new FontIconSource() { Glyph = "\uE82D" }, new Guid());
    group3.Add(new NavigationBoardItem("Group 3 Board 1", new FontIconSource() { Glyph = "\uE737" }, new Guid()));
    group3.Add(new NavigationBoardItem("Group 3 Board 2", new FontIconSource() { Glyph = "\uE737" }, new Guid()));
    group3.Add(new NavigationBoardItem("Group 3 Board 3", new FontIconSource() { Glyph = "\uE737" }, new Guid()));
    group1.Add(group3);

    ListMenuRootItem.Add(group1, group2);
    ListMenuItems = ListMenuRootItem.Children;

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

  public NavigationBoardGroupItem ListMenuRootItem { get; } = new("Root", new FontIconSource() { Glyph = "\uE82D" }, new Guid());
  public ReadOnlyObservableCollection<NavigationBoardItem> ListMenuItems { get; }
  public List<Navigation> FooterMenuItems { get; } =
  [
    new NavigationTrashItem(),
    new NavigationSettingsItem()
  ];

  public void Recursive(NavigationBoardItem item, Action<NavigationBoardItem> action)
  {
    if (item is NavigationBoardGroupItem group)
      foreach (NavigationBoardItem child in group.Children)
        Recursive(child, action);
    action.Invoke(item);
  }

  public void MoveNavigationBoardItem(NavigationBoardItem source, NavigationBoardItem target, NavigationBoardItemPosition position)
  {
    NavigationBoardGroupItem? sourceGroup = source.Parent;
    NavigationBoardGroupItem? targetGroup = target.Parent;
    switch (position)
    {
      case NavigationBoardItemPosition.Before:
        targetGroup?.Insert(targetGroup.Children.IndexOf(target), source);
        break;
      case NavigationBoardItemPosition.After:
        targetGroup?.Insert(targetGroup.Children.IndexOf(target) + 1, source);
        break;
    }
  }
}

public enum NavigationBoardItemPosition { Before, After, Inside }