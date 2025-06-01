using System.Collections.Immutable;

using MyNotes.Common.Commands;
using MyNotes.Common.Messaging;
using MyNotes.Core.Models;
using MyNotes.Core.Services;

namespace MyNotes.Core.ViewModels;

public class MainViewModel : ViewModelBase
{
  public MainViewModel(DatabaseService databaseService)
  {
    DatabaseService = databaseService;

    InitializeNavigationsFromDatabase();

    CurrentNavigation = (NavigationItem)NavigationCoreMenus[0];
    NavigationBoardMenus = NavigationBoardRootMenu.Children;
    MenuItems.Source = new List<IList>() { NavigationCoreMenus, NavigationBoardMenus };
    SetCommands();
  }

  ~MainViewModel()
  {
    TraverseNavigationBoardMenusAll(NavigationBoardRootMenu, (navigation) =>
    {
      if (navigation is NavigationBoardGroup group)
        group.ChildrenChanged -= OnNavigationBoardGroupChanged;
    });
  }

  private readonly DatabaseService DatabaseService;

  public NavigationItem? CurrentNavigation { get; set; }
  public NavigationBoard? TargetNavigation { get; set; }

  public CollectionViewSource MenuItems { get; } = new() { IsSourceGrouped = true };

  public ImmutableList<Navigation> NavigationCoreMenus { get; } =
  [
    new NavigationHome(),
    new NavigationBookmarks(),
    new NavigationTags(),
    new NavigationSeparator(),
  ];

  public NavigationBoardRootGroup NavigationBoardRootMenu { get; } = new();
  public ReadOnlyObservableCollection<NavigationBoard> NavigationBoardMenus { get; }

  public ImmutableList<Navigation> NavigationFooterMenus { get; } =
  [
    new NavigationTrash(),
    new NavigationSettings()
  ];

  private void InitializeNavigationsFromDatabase()
  {
    DatabaseService.BuildNavigationBoardTree(NavigationBoardRootMenu);
    TraverseNavigationBoardMenusAll(NavigationBoardRootMenu, (navigation) =>
    {
      if (navigation is NavigationBoardGroup group)
        group.ChildrenChanged += OnNavigationBoardGroupChanged;
    });
  }

  private bool _isEditMode = false;
  public bool IsEditMode
  {
    get => _isEditMode;
    set => SetProperty(ref _isEditMode, value);
  }

  private void OnNavigationBoardGroupChanged(object? sender, NotifyCollectionChangedEventArgs e)
  {
    if (IsEditMode)
      return;
    var oldItems = e.OldItems?.Cast<NavigationBoard>();
    var newItems = e.NewItems?.Cast<NavigationBoard>();
    switch (e.Action)
    {
      case NotifyCollectionChangedAction.Add:
        foreach (var newItem in newItems!)
        {
          if (newItem is NavigationBoardGroup newGroup)
            newGroup.ChildrenChanged += OnNavigationBoardGroupChanged;
          DatabaseService.AddBoard(newItem);
        }
        break;
      case NotifyCollectionChangedAction.Remove:
        foreach (var oldItem in oldItems!)
        {
          if (oldItem is NavigationBoardGroup oldGroup)
            oldGroup.ChildrenChanged -= OnNavigationBoardGroupChanged;
          DatabaseService.DeleteBoard(oldItem);
        }
        break;
      case NotifyCollectionChangedAction.Replace:
        break;
      case NotifyCollectionChangedAction.Move:
        break;
      case NotifyCollectionChangedAction.Reset:
        break;
    }
  }

  public void TraverseNavigationBoardMenusAll(NavigationBoard root, Action<NavigationBoard> action, TreeTraversalOrder traversalOrder = TreeTraversalOrder.Pre)
  {
    if (traversalOrder == TreeTraversalOrder.Pre)
      TraverseNavigationBoardMenusAllPreOrder(root, action);
    else if (traversalOrder == TreeTraversalOrder.Post)
      TraverseNavigationBoardMenusAllPostOrder(root, action);
  }

  private void TraverseNavigationBoardMenusAllPreOrder(NavigationBoard root, Action<NavigationBoard> action)
  {
    Stack<NavigationBoard> stack = new();
    stack.Push(root);

    while (stack.Count > 0)
    {
      NavigationBoard navigation = stack.Pop();
      action(navigation);

      if (navigation is NavigationBoardGroup group)
      {
        int groupCount = group.ChildCount;
        for (int index = groupCount - 1; index >= 0; index--)
          stack.Push(group.GetChild(index));
      }
    }
  }

  private void TraverseNavigationBoardMenusAllPostOrder(NavigationBoard root, Action<NavigationBoard> action)
  {
    Stack<NavigationBoard> stack1 = new();
    Stack<NavigationBoard> stack2 = new();
    stack1.Push(root);

    while (stack1.Count > 0)
    {
      NavigationBoard navigation = stack1.Pop();
      stack2.Push(navigation);

      if (navigation is NavigationBoardGroup group)
        foreach(var child in group.Children)
          stack1.Push(child);
    }

    while (stack2.Count > 0)
      action(stack2.Pop());
  }

  public bool TraverseNavigationBoardMenusAny(NavigationBoard root, Predicate<NavigationBoard> predicate)
  {
    Stack<NavigationBoard> stack = new();
    stack.Push(root);

    while (stack.Count > 0)
    {
      NavigationBoard navigation = stack.Pop();
      if (predicate(navigation))
        return true;

      if (navigation is NavigationBoardGroup group)
      {
        int groupCount = group.ChildCount;
        for (int index = groupCount - 1; index >= 0; index--)
          stack.Push(group.GetChild(index));
      }
    }
    return false;
  }

  public bool ContainsBoard(NavigationBoard navigation, NavigationBoardGroup? rootGroup = null)
  {
    rootGroup ??= NavigationBoardRootMenu;
    return TraverseNavigationBoardMenusAny(rootGroup, (item) => item == navigation);
  }

  public void ResetNavigation()
  {
    foreach (NavigationBoard child in NavigationBoardRootMenu.Children)
      TraverseNavigationBoardMenusAll(child, item => DatabaseService.UpdateBoardHierarchy(item, item.GetNext()));
  }

  public void MoveNavigationBoardMenu(NavigationBoard source, NavigationBoard target, NavigationBoardItemPosition position)
  {
    NavigationBoardGroup? sourceGroup = source.Parent;
    NavigationBoardGroup? targetGroup = target.Parent;
    switch (position)
    {
      case NavigationBoardItemPosition.Before:
        targetGroup?.InsertChild(targetGroup.Children.IndexOf(target), source);
        break;
      case NavigationBoardItemPosition.After:
        targetGroup?.InsertChild(targetGroup.Children.IndexOf(target) + 1, source);
        break;
    }
  }

  #region Rename
  private string _boardNameToRename = "";
  public string BoardNameToRename
  {
    get => _boardNameToRename;
    set => SetProperty(ref _boardNameToRename, value);
  }

  private string _noteTitleToRename = "";
  public string NoteTitleToRename
  {
    get => _noteTitleToRename;
    set => SetProperty(ref _noteTitleToRename, value);
  }
  #endregion

  #region New Board
  public NavigationBoardGroup NavigationGroupToAdd { get; set; } = null!;
  private string _newBoardName = "";
  public string NewBoardName
  {
    get => _newBoardName;
    set => SetProperty(ref _newBoardName, value);
  }

  private string _newBoardGroupName = "";
  public string NewBoardGroupName
  {
    get => _newBoardGroupName;
    set => SetProperty(ref _newBoardGroupName, value);
  }

  private Icon _newBoardIcon = IconLibrary.FindGlyph("\uE70B");
  public Icon NewBoardIcon
  {
    get => _newBoardIcon;
    set => SetProperty(ref _newBoardIcon, value);
  }

  private Icon _newBoardGroupIcon = IconLibrary.FindGlyph("\uE82D");
  public Icon NewBoardGroupIcon
  {
    get => _newBoardGroupIcon;
    set => SetProperty(ref _newBoardGroupIcon, value);
  }
  #endregion

  #region Rename
  public void RenameNavigationBoardMenu(NavigationBoard navigation, string newName)
  {
    if (string.IsNullOrEmpty(newName) || navigation.Name == newName)
      return;
    navigation.Name = newName;
    DatabaseService.UpdateBoard(navigation, "Name");
  }
  #endregion

  #region Remove
  public void RemoveNavigationBoardMenu(NavigationBoard navigation)
  {
    if (CurrentNavigation == navigation)
      WeakReferenceMessenger.Default.Send(new Message(this), Tokens.GoToNavigationHome);
    TraverseNavigationBoardMenusAny(NavigationBoardRootMenu, (item) =>
    {
      if (item == navigation)
      {
        TraverseNavigationBoardMenusAll(item, (board) => board.Parent?.RemoveChild(board), TreeTraversalOrder.Post);
        return true;
      }
      return false;
    });
  }
  #endregion

  #region Set Commands
  public Command<NavigationViewDisplayMode>? ChangeContentAlignmentCommand { get; private set; }
  public Command? AddNavigationBoardCommand { get; private set; }
  public Command? AddNavigationBoardGroupCommand { get; private set; }
  public Command? SetNavigationEditModeCommand { get; private set; }

  public void SetCommands()
  {
    ChangeContentAlignmentCommand = new((mode) =>
    {
      if (CurrentNavigation is NavigationNotes navigation)
      {
        NoteBoardViewModel viewModel = App.Current.GetService<NoteBoardViewModelFactory>().Create(navigation);
        if (mode == NavigationViewDisplayMode.Minimal)
          WeakReferenceMessenger.Default.Send(new Message(viewModel, "GridViewAlignCenter"), Tokens.ChangeNoteBoardGridViewAlignment);
        else
          WeakReferenceMessenger.Default.Send(new Message(viewModel, "GridViewAlignStretch"), Tokens.ChangeNoteBoardGridViewAlignment);
      }
    });

    AddNavigationBoardCommand = new(() =>
    {
      NavigationBoard item = new(NewBoardName, NewBoardIcon, Guid.NewGuid());
      NavigationGroupToAdd.AddChild(item);
    });

    AddNavigationBoardGroupCommand = new(() =>
    {
      NavigationBoardGroup item = new(NewBoardGroupName, NewBoardGroupIcon, Guid.NewGuid());
      NavigationGroupToAdd.AddChild(item);
    });

    SetNavigationEditModeCommand = new(() =>
    {
      IsEditMode = !IsEditMode;

      foreach (Navigation item in NavigationCoreMenus)
        item.IsEditable = IsEditMode;
      foreach (Navigation item in NavigationFooterMenus)
        item.IsEditable = IsEditMode;
      foreach (NavigationBoard item in NavigationBoardMenus)
        TraverseNavigationBoardMenusAll(item, x => x.IsEditable = IsEditMode);

      if (!IsEditMode)
        ResetNavigation();

      WeakReferenceMessenger.Default.Send(new Message(this, IsEditMode), Tokens.ChangeNavigationViewSelection);
    });
  }
  #endregion
}

public enum NavigationBoardItemPosition { Before, After, Inside }