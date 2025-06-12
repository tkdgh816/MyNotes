using System.Collections.Immutable;

using MyNotes.Common.Commands;
using MyNotes.Common.Messaging;
using MyNotes.Core.Dto;
using MyNotes.Core.Model;
using MyNotes.Core.Service;

namespace MyNotes.Core.ViewModel;

internal class MainViewModel : ViewModelBase
{
  public MainViewModel(NavigationService navigationService)
  {
    _navigationService = navigationService;

    _navigationService.BuildNavigationTree(UserRootGroup);
    _navigationService.SetCurrentNavigation((NavigationItem)CoreMenus[0]);
    UserBoards = UserRootGroup.Children;
    MenuItems = [CoreMenus, UserBoards];

    SetCommands();
    RegisterEvents();
    RegisterMessengers();
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing)
    {
      UnregisterEvents();
      UnregisterMessengers();
    }
    base.Dispose(disposing);
  }

  private readonly NavigationService _navigationService;

  public ImmutableList<Navigation> CoreMenus { get; } =
  [
    new NavigationHome(),
    new NavigationBookmarks(),
    new NavigationTags(),
    new NavigationSeparator(),
  ];

  public NavigationUserRootGroup UserRootGroup { get; } = new();
  public ReadOnlyObservableCollection<NavigationUserBoard> UserBoards { get; }

  public ImmutableList<Navigation> FooterMenus { get; } =
  [
    new NavigationTrash(),
    new NavigationSettings()
  ];

  public List<IList> MenuItems { get; }

  #region Handling Events
  private void RegisterEvents()
  {
    TraverseNavigationBoardMenusAll(UserRootGroup, (navigation) =>
    {
      navigation.PropertyChanged += OnNavigationPropertyChanged;
      if (navigation is NavigationUserGroup group)
        group.ChildrenChanged += OnNavigationBoardGroupChanged;
    });
  }

  private void UnregisterEvents()
  {
    TraverseNavigationBoardMenusAll(UserRootGroup, (navigation) =>
    {
      navigation.PropertyChanged -= OnNavigationPropertyChanged;
      if (navigation is NavigationUserGroup group)
        group.ChildrenChanged -= OnNavigationBoardGroupChanged;
    });
  }

  private void OnNavigationPropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (e.PropertyName == "Icon")
      _navigationService.UpdateBoard((NavigationUserBoard)sender!, BoardUpdateFields.IconType | BoardUpdateFields.IconValue);
  }

  private void OnNavigationBoardGroupChanged(object? sender, NotifyCollectionChangedEventArgs e)
  {
    if (IsEditMode)
      return;
    var oldItems = e.OldItems?.Cast<NavigationUserBoard>();
    var newItems = e.NewItems?.Cast<NavigationUserBoard>();
    switch (e.Action)
    {
      case NotifyCollectionChangedAction.Add:
        foreach (var newItem in newItems!)
        {
          if (newItem is NavigationUserGroup newGroup)
            newGroup.ChildrenChanged += OnNavigationBoardGroupChanged;
          _navigationService.AddBoard(newItem);
        }
        break;
      case NotifyCollectionChangedAction.Remove:
        foreach (var oldItem in oldItems!)
        {
          if (oldItem is NavigationUserGroup oldGroup)
            oldGroup.ChildrenChanged -= OnNavigationBoardGroupChanged;
          _navigationService.DeleteBoard(oldItem);
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
  #endregion

  private bool _isEditMode = false;
  public bool IsEditMode
  {
    get => _isEditMode;
    set => SetProperty(ref _isEditMode, value);
  }

  public void TraverseNavigationBoardMenusAll(NavigationUserBoard root, Action<NavigationUserBoard> action, TreeTraversalOrder traversalOrder = TreeTraversalOrder.Pre)
  {
    if (traversalOrder == TreeTraversalOrder.Pre)
      TraverseNavigationBoardMenusAllPreOrder(root, action);
    else if (traversalOrder == TreeTraversalOrder.Post)
      TraverseNavigationBoardMenusAllPostOrder(root, action);
  }

  private void TraverseNavigationBoardMenusAllPreOrder(NavigationUserBoard root, Action<NavigationUserBoard> action)
  {
    Stack<NavigationUserBoard> stack = new();
    stack.Push(root);

    while (stack.Count > 0)
    {
      NavigationUserBoard navigation = stack.Pop();
      action(navigation);

      if (navigation is NavigationUserGroup group)
      {
        int groupCount = group.ChildCount;
        for (int index = groupCount - 1; index >= 0; index--)
          stack.Push(group.GetChild(index));
      }
    }
  }

  private void TraverseNavigationBoardMenusAllPostOrder(NavigationUserBoard root, Action<NavigationUserBoard> action)
  {
    Stack<NavigationUserBoard> stack1 = new();
    Stack<NavigationUserBoard> stack2 = new();
    stack1.Push(root);

    while (stack1.Count > 0)
    {
      NavigationUserBoard navigation = stack1.Pop();
      stack2.Push(navigation);

      if (navigation is NavigationUserGroup group)
        foreach (var child in group.Children)
          stack1.Push(child);
    }

    while (stack2.Count > 0)
      action(stack2.Pop());
  }

  public bool TraverseNavigationBoardMenusAny(NavigationUserBoard root, Predicate<NavigationUserBoard> predicate)
  {
    Stack<NavigationUserBoard> stack = new();
    stack.Push(root);

    while (stack.Count > 0)
    {
      NavigationUserBoard navigation = stack.Pop();
      if (predicate(navigation))
        return true;

      if (navigation is NavigationUserGroup group)
      {
        int groupCount = group.ChildCount;
        for (int index = groupCount - 1; index >= 0; index--)
          stack.Push(group.GetChild(index));
      }
    }
    return false;
  }

  public bool ContainsBoard(NavigationUserBoard navigation, NavigationUserGroup? rootGroup = null)
  {
    rootGroup ??= UserRootGroup;
    return TraverseNavigationBoardMenusAny(rootGroup, (item) => item == navigation);
  }

  public void RefreshNavigation()
  {
    foreach (NavigationUserBoard child in UserRootGroup.Children)
      TraverseNavigationBoardMenusAll(child, item => _navigationService.UpdateBoard(item, BoardUpdateFields.Parent | BoardUpdateFields.Previous));
  }

  public void MoveNavigationBoardMenu(NavigationUserBoard source, NavigationUserBoard target, NavigationBoardItemPosition position)
  {
    NavigationUserGroup? sourceGroup = source.Parent;
    NavigationUserGroup? targetGroup = target.Parent;
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
  public NavigationUserGroup NavigationGroupToAdd { get; set; } = null!;
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
  public void RenameNavigationBoardMenu(NavigationUserBoard navigation, string newName)
  {
    if (string.IsNullOrEmpty(newName) || navigation.Name == newName)
      return;
    navigation.Name = newName;
    _navigationService.UpdateBoard(navigation, BoardUpdateFields.Name);
  }
  #endregion

  #region Remove
  public void RemoveNavigationBoardMenu(NavigationUserBoard navigation)
  {
    if (_navigationService.CurrentNavigation == navigation)
      _navigationService.Navigate((NavigationItem)CoreMenus[0]);
    TraverseNavigationBoardMenusAll(navigation, (board) => board.Parent?.RemoveChild(board), TreeTraversalOrder.Post);
  }
  #endregion

  #region Set Commands
  public Command<NavigationItem>? NavigateCommand { get; private set; }
  public Command<string>? SearchNavigationCommand { get; private set; }
  public Command? GoBackCommand { get; private set; }
  public Command<NavigationViewDisplayMode>? ChangeContentAlignmentCommand { get; private set; }
  public Command? AddNavigationBoardCommand { get; private set; }
  public Command? AddNavigationBoardGroupCommand { get; private set; }
  public Command? SetNavigationEditModeCommand { get; private set; }
  public Command<NavigationUserBoard>? MoveNoteToBoardCommand { get; private set; }

  public void SetCommands()
  {
    NavigateCommand = new(navigation => _navigationService.Navigate(navigation));

    SearchNavigationCommand = new((queryText) =>
    {
      _navigationService.Navigate(new NavigationSearch(queryText));
    });

    GoBackCommand = new(() => _navigationService.GoBack());

    ChangeContentAlignmentCommand = new((mode) =>
    {
      if (_navigationService.CurrentNavigation is NavigationBoard navigation)
      {
        BoardViewModel viewModel = App.Current.GetService<BoardViewModelFactory>().Create(navigation);
        if (mode == NavigationViewDisplayMode.Minimal)
          WeakReferenceMessenger.Default.Send(new Message<string>("GridViewAlignCenter", viewModel), Tokens.ChangeNoteBoardGridViewAlignment);
        else
          WeakReferenceMessenger.Default.Send(new Message<string>("GridViewAlignStretch", viewModel), Tokens.ChangeNoteBoardGridViewAlignment);
      }
    });

    AddNavigationBoardCommand = new(() =>
    {
      NavigationUserBoard item = new(NewBoardName, NewBoardIcon, Guid.NewGuid());
      NavigationGroupToAdd.AddChild(item);
    });

    AddNavigationBoardGroupCommand = new(() =>
    {
      NavigationUserGroup item = new(NewBoardGroupName, NewBoardGroupIcon, Guid.NewGuid());
      NavigationGroupToAdd.AddChild(item);
    });

    SetNavigationEditModeCommand = new(() =>
    {
      IsEditMode = !IsEditMode;

      foreach (Navigation item in CoreMenus)
        item.IsEditable = IsEditMode;
      foreach (Navigation item in FooterMenus)
        item.IsEditable = IsEditMode;
      foreach (NavigationUserBoard item in UserBoards)
        TraverseNavigationBoardMenusAll(item, x => x.IsEditable = IsEditMode);

      if (!IsEditMode)
        RefreshNavigation();

      _navigationService.ChangeNavigationViewSelectionWithoutNavigation(IsEditMode ? null : _navigationService.CurrentNavigation);
    });

    MoveNoteToBoardCommand = new((board) =>
    {
      Debug.WriteLine(board.Name);
    });
  }
  #endregion

  private void RegisterMessengers()
  {
    WeakReferenceMessenger.Default.Register<ExtendedRequestMessage<NavigationUserBoard, bool>, string>(this, Tokens.IsValidNavigation, new((recipient, message) =>
    {
      message.Reply(ContainsBoard(message.Request));
    }));
  }

  private void UnregisterMessengers() => WeakReferenceMessenger.Default.UnregisterAll(this);
}

internal enum NavigationBoardItemPosition { Before, After, Inside }