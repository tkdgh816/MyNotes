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
    new NavigationHome(),
    new NavigationBookmarks(),
    new NavigationTags(),
    new NavigationSeparator(),
  ];

  public NavigationBoardRootGroup BoardMenuRootItem { get; } = new();
  public ReadOnlyObservableCollection<NavigationBoard> BoardMenuItems { get; }
  public List<Navigation> FooterMenuItems { get; } =
  [
    new NavigationTrash(),
    new NavigationSettings()
  ];

  private void InitializeNavigationsFromDatabase()
  {
    DatabaseService.GetNavigationGroup(BoardMenuRootItem);
  }

  public void Recursive(NavigationBoard item, Action<NavigationBoard> action)
  {
    if (item is NavigationBoardGroup group)
      foreach (NavigationBoard child in group.Children)
        Recursive(child, action);
    action.Invoke(item);
  }

  public void ResetNavigation()
  {
    foreach (NavigationBoard child in BoardMenuRootItem.Children)
      Recursive(child, item => DatabaseService.UpdateBoardHierarchy(item, item.GetNext()));
  }

  public void MoveNavigationBoardItem(NavigationBoard source, NavigationBoard target, NavigationBoardItemPosition position)
  {
    NavigationBoardGroup? sourceGroup = source.Parent;
    NavigationBoardGroup? targetGroup = target.Parent;
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

  public NavigationBoard AddNavigationBoardItem(NavigationBoardGroup parent, string name, bool isGroupItem)
  {
    NavigationBoard item = isGroupItem
      ? new NavigationBoardGroup(name, NewBoardGroupIcon, Guid.NewGuid())
      : new NavigationBoard(name, NewBoardIcon, Guid.NewGuid());
    parent.Add(item);
    DatabaseService.AddBoard(item, parent.Children.Count > 1 ? parent.Children[^2] : null);
    return item;
  }

  public void RenameNavigation(NavigationBoard item, string newName)
  {
    if (string.IsNullOrEmpty(newName) || item.Name == newName)
      return;
    item.Name = newName;
    DatabaseService.UpdateBoard(item, "Name");
  }

  public void RegisterNavigation(NavigationItem item) => NavigationService.CurrentNavigation = item;
}

public enum NavigationBoardItemPosition { Before, After, Inside }