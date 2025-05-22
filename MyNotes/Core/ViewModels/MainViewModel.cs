using MyNotes.Core.Models;
using MyNotes.Core.Services;

namespace MyNotes.Core.ViewModels;

public class MainViewModel : ViewModelBase
{
  public MainViewModel(DatabaseService databaseService, NavigationService navigationService)
  {
    DatabaseService = databaseService;
    NavigationService = navigationService;

    InitializeNavigationsFromDatabase();
    BoardMenuItems = BoardMenuRootItem.Children;
    MenuItems.Source = new List<IList>() { CoreMenuItems, BoardMenuItems };
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

  public NavigationBoardGroupRootItem BoardMenuRootItem { get; } = new();
  public ReadOnlyObservableCollection<NavigationBoardItem> BoardMenuItems { get; }
  public List<Navigation> FooterMenuItems { get; } =
  [
    new NavigationTrashItem(),
    new NavigationSettingsItem()
  ];

  private void InitializeNavigationsFromDatabase()
  {
    DatabaseService.GetNavigationGroup(BoardMenuRootItem);
  }

  public void Recursive(NavigationBoardItem item, Action<NavigationBoardItem> action)
  {
    if (item is NavigationBoardGroupItem group)
      foreach (NavigationBoardItem child in group.Children)
        Recursive(child, action);
    action.Invoke(item);
  }

  public void ResetNavigation()
  {
    foreach (NavigationBoardItem child in BoardMenuRootItem.Children)
      Recursive(child, item => DatabaseService.UpdateBoardHierarchy(item, item.GetNext()));
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

  private Icon _newBoardGroupIcon = IconLibrary.FindGlyph("\uE82D");
  public Icon NewBoardGroupIcon
  {
    get => _newBoardGroupIcon;
    set => SetProperty(ref _newBoardGroupIcon, value);
  }

  private Icon _newBoardIcon = IconLibrary.FindGlyph("\uE70B");
  public Icon NewBoardIcon
  {
    get => _newBoardIcon;
    set => SetProperty(ref _newBoardIcon, value);
  }

  public NavigationBoardItem AddNavigationBoardItem(NavigationBoardGroupItem parent, string name, bool isGroupItem)
  {
    NavigationBoardItem item = isGroupItem
      ? new NavigationBoardGroupItem(name, NewBoardGroupIcon, Guid.NewGuid())
      : new NavigationBoardItem(name, NewBoardIcon, Guid.NewGuid());
    parent.Add(item);
    DatabaseService.AddBoard(item, parent.Children.Count > 1 ? parent.Children[^2] : null);
    return item;
  }

  public void RenameNavigation(NavigationBoardItem item, string newName)
  {
    if (string.IsNullOrEmpty(newName) || item.Name == newName)
      return;
    item.Name = newName;
    DatabaseService.UpdateBoard(item, "Name");
  }

  public void RegisterNavigation(NavigationItem item) => NavigationService.CurrentNavigation = item;
}

public enum NavigationBoardItemPosition { Before, After, Inside }