using MyNotes.Common.Commands;
using MyNotes.Common.Messaging;
using MyNotes.Core.Dto;
using MyNotes.Core.Model;
using MyNotes.Core.Service;
using MyNotes.Core.Shared;

namespace MyNotes.Core.ViewModel;

internal class MainViewModel : ViewModelBase
{
  private readonly NavigationService _navigationService;
  private readonly DialogService _dialogService;
  private readonly NoteService _noteService;
  private readonly NoteViewModelFactory _noteViewModelFactory;

  public MainViewModel(NavigationService navigationService, DialogService dialogService, NoteService noteService, NoteViewModelFactory noteViewModelFactory)
  {
    _navigationService = navigationService;
    _dialogService = dialogService;
    _noteService = noteService;
    _noteViewModelFactory = noteViewModelFactory;

    _navigationService.BuildNavigationTree(UserRootNavigation);
    _navigationService.SetCurrentNavigation((NavigationItem)CoreNavigations[0]);

    MenuItems = [CoreNavigations, UserNavigations];

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

  #region 내비게이션 컬렉션
  // 상단 고정 영역: 홈, 북마크, 태그, 구분선
  // 사용자 영역: 사용자 보드와 그룹
  // 하단 고정 영역: 휴지통, 설정
  private readonly ImmutableList<Navigation> CoreNavigations =
  [
    new NavigationHome(),
    new NavigationBookmarks(),
    new NavigationTags(),
    new NavigationSeparator(),
  ];

  public NavigationUserRootGroup UserRootNavigation { get; } = new();
  private IReadOnlyList<NavigationUserBoard> UserNavigations => UserRootNavigation.Children;

  private readonly ImmutableList<Navigation> SecondaryNavigations =
  [
    new NavigationTrash(),
    new NavigationSettings()
  ];

  // 뷰에 바인딩하기 위한 노출 ReadOnly Collection 정의
  // 공변성 사용하여 Navigation으로 암시적 참조 변환
  public IReadOnlyList<IReadOnlyList<Navigation>> MenuItems { get; }
  public IReadOnlyList<Navigation> FooterMenuItems => SecondaryNavigations;
  #endregion

  #region Handling Events
  private void RegisterEvents()
  {
    TraverseUserNavigationsAll(UserRootNavigation, (navigation) =>
    {
      navigation.PropertyChanged += OnNavigationPropertyChanged;
      if (navigation is NavigationUserGroup group)
        group.ChildrenChanged += OnNavigationBoardGroupChanged;
    });
  }

  private void UnregisterEvents()
  {
    TraverseUserNavigationsAll(UserRootNavigation, (navigation) =>
    {
      navigation.PropertyChanged -= OnNavigationPropertyChanged;
      if (navigation is NavigationUserGroup group)
        group.ChildrenChanged -= OnNavigationBoardGroupChanged;
    });
  }

  // User Navigation 속성 변경 시
  // 데이터베이스에 이를 업데이트하는 이벤트 핸들러
  private void OnNavigationPropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (e.PropertyName == "Name")
      _navigationService.UpdateBoard((NavigationUserBoard)sender!, BoardUpdateFields.Name);
    else if (e.PropertyName == "Icon")
      _navigationService.UpdateBoard((NavigationUserBoard)sender!, BoardUpdateFields.IconType | BoardUpdateFields.IconValue);
  }

  // Group User Navigation 자식 요소의 컬렉션 항목 추가, 제거 등의 변경 발생 시
  // 데이터베이스에 이를 업데이트하는 이벤트 핸들러
  // Navigation 이동 모드 시에는 데이터베이스에 실시간 업데이트하지 않음
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

  // 루트 노드 내에 내비게이션이 있는지 확인하는 메서드
  public bool ContainsUserNavigation(NavigationUserBoard navigation, NavigationUserGroup? rootGroup = null)
  {
    rootGroup ??= UserRootNavigation;
    return TraverseUserNavigationsAny(rootGroup, (item) => item == navigation);
  }

  // User Navigation을 다른 노드 앞 혹은 뒤로 이동시키는 메서드
  // 소스 노드르 타겟 노드의 부모 노드의 자식으로 이동
  public void MoveUserNavigation(NavigationUserBoard source, NavigationUserBoard target, NavigationBoardItemPosition position)
  {
    NavigationUserGroup? targetGroup = target.Parent;
    switch (position)
    {
      case NavigationBoardItemPosition.Before:
        targetGroup?.InsertChild(targetGroup.IndexOfChild(target), source);
        break;
      case NavigationBoardItemPosition.After:
        targetGroup?.InsertChild(targetGroup.IndexOfChild(target) + 1, source);
        break;
      case NavigationBoardItemPosition.Inside:
        if (target is NavigationUserGroup group)
          group.InsertChild(0, source);
        break;
    }
  }

  // UserNavigations 트리의 모든 노드의 관계를 데이터베이스에 갱신하는 메서드
  public void UpdateUserNavigationRelations()
  {
    foreach (NavigationUserBoard child in UserRootNavigation.Children)
      TraverseUserNavigationsAll(child, item => _navigationService.UpdateBoard(item, BoardUpdateFields.Parent | BoardUpdateFields.Previous));
  }

  // User Navigation의 이름 변경하는 메서드
  public void RenameUserNavigation(NavigationUserBoard navigation, string newName)
  {
    if (string.IsNullOrEmpty(newName) || navigation.Name == newName)
      return;
    navigation.Name = newName;
    _navigationService.UpdateBoard(navigation, BoardUpdateFields.Name);
  }

  // User Navigation 삭제하는 메서드
  // 하위 노드들도 함께 삭제해야 하고
  // 현재 페이지가 삭제하려는 노드라면 홈으로 이동 후 삭제
  public void DeleteUserNavigation(NavigationUserBoard navigation)
  {
    if (_navigationService.CurrentNavigation == navigation)
      _navigationService.Navigate((NavigationItem)CoreNavigations[0]);
    TraverseUserNavigationsAll(navigation, (board) => board.Parent?.RemoveChild(board), TreeTraversalOrder.Post);
  }

  // NoteId와 BoardId를 받아서 해당 NoteViewModel을 가져온 후
  // Note의 BoardId를 변경시키는 메서드
  public void MoveNoteToBoard(NoteId noteId, BoardId boardId)
  {
    var note = _noteService.GetNote(noteId);
    if (note is not null)
    {
      var noteViewModel = _noteViewModelFactory.Resolve(note);
      noteViewModel.MoveNoteToBoardCommand?.Execute(boardId);
    }
  }

  #region Commands
  public Command<NavigationItem>? NavigateCommand { get; private set; }
  public Command<string>? SearchNavigationCommand { get; private set; }
  public Command? GoBackCommand { get; private set; }
  public Command? SetNavigationEditModeCommand { get; private set; }
  public Command<NavigationUserGroup>? ShowNewBoardDialogCommand { get; private set; }
  public Command<NavigationUserGroup>? ShowNewGroupDialogCommand { get; private set; }
  public Command<NavigationUserBoard>? ShowRenameBoardDialogCommand { get; private set; }
  public Command<NavigationUserBoard>? ShowDeleteBoardDialogCommand { get; private set; }

  public void SetCommands()
  {
    NavigateCommand = new(navigation => _navigationService.Navigate(navigation));

    SearchNavigationCommand = new((queryText) =>
    {
      _navigationService.Navigate(new NavigationSearch(queryText));
    });

    GoBackCommand = new(() => _navigationService.GoBack());

    SetNavigationEditModeCommand = new(() =>
    {
      IsEditMode = !IsEditMode;

      foreach (Navigation item in CoreNavigations)
        item.IsEditable = IsEditMode;
      foreach (Navigation item in SecondaryNavigations)
        item.IsEditable = IsEditMode;
      foreach (NavigationUserBoard item in UserNavigations)
        TraverseUserNavigationsAll(item, x => x.IsEditable = IsEditMode);

      if (!IsEditMode)
        UpdateUserNavigationRelations();

      _navigationService.ChangeNavigationViewSelectionWithoutNavigation(IsEditMode ? null : _navigationService.CurrentNavigation);
      _navigationService.ToggleNavigationViewContentEnabled(!IsEditMode);
    });

    ShowNewBoardDialogCommand = new(async (group) =>
    {
      var result = await _dialogService.ShowCreateBoardDialog();
      if (result.DialogResult)
        group.AddChild(new NavigationUserBoard(result.Name!, result.Icon!, new BoardId(Guid.NewGuid())));
    });

    ShowNewGroupDialogCommand = new(async (group) =>
    {
      var result = await _dialogService.ShowCreateBoardGroupDialog();
      if (result.DialogResult)
        group.AddChild(new NavigationUserGroup(result.Name!, result.Icon!, new BoardId(Guid.NewGuid())));
    });

    ShowRenameBoardDialogCommand = new(async (navigation) =>
    {
      var result = await _dialogService.ShowRenameBoardDialog(navigation);
      if (result.DialogResult)
      {
        if (result.Icon is not null)
          navigation.Icon = result.Icon;
        if (result.Name is not null)
          navigation.Name = result.Name;
      }
    });

    ShowDeleteBoardDialogCommand = new(async (navigation) =>
    {
      if (await _dialogService.ShowDeleteBoardDialog(navigation))
        DeleteUserNavigation(navigation);
    });
  }
  #endregion

  #region Messengers
  private void RegisterMessengers()
  {
    WeakReferenceMessenger.Default.Register<ExtendedRequestMessage<NavigationUserBoard, bool>, string>(this, Tokens.IsValidNavigation, new((recipient, message) => message.Reply(ContainsUserNavigation(message.Request))));
  }

  private void UnregisterMessengers() => WeakReferenceMessenger.Default.UnregisterAll(this);
  #endregion

  #region 내비게이션 순회 로직
  // 지정한 루트 내비게이션 노드와 그 하위 노드를 순회하여 모든 노드에 Action 수행
  // 순회 방법: Pre -> 전위 순회(루트 노드 우선), Post -> 후위 순회(자식 노드 우선)
  public void TraverseUserNavigationsAll(NavigationUserBoard root, Action<NavigationUserBoard> action, TreeTraversalOrder traversalOrder = TreeTraversalOrder.Pre)
  {
    if (traversalOrder == TreeTraversalOrder.Pre)
      TraverseUserNavigationsAllPreOrder(root, action);
    else if (traversalOrder == TreeTraversalOrder.Post)
      TraverseUserNavigationsAllPostOrder(root, action);
  }

  private void TraverseUserNavigationsAllPreOrder(NavigationUserBoard root, Action<NavigationUserBoard> action)
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

  private void TraverseUserNavigationsAllPostOrder(NavigationUserBoard root, Action<NavigationUserBoard> action)
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

  // 지정한 루트 내비게이션 노드와 그 하위 노드를 순회하여
  // Predicate 조건을 만족하는 노드가 하나라도 있다면 true 반환
  public bool TraverseUserNavigationsAny(NavigationUserBoard root, Predicate<NavigationUserBoard> predicate)
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
  #endregion
}

internal enum NavigationBoardItemPosition { Before, After, Inside }