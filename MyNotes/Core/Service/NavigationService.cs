using MyNotes.Common.Messaging;
using MyNotes.Core.Dto;
using MyNotes.Core.Model;
using MyNotes.Core.Shared;
using MyNotes.Debugging;

namespace MyNotes.Core.Service;

internal class NavigationService
{
  public NavigationService(DatabaseService databaseService)
  {
    _databaseService = databaseService;
  }

  private readonly DatabaseService _databaseService;

  #region NavigationUserBoard와 Database 관련 로직 (트리 생성, 추가, 제거, 업데이트)
  // 데이터베이스에서 트리 구조 가져와서 사용자 보드, 그룹 트리 구조 생성
  public void BuildNavigationTree(NavigationUserRootGroup root)
  {
    var boards = _databaseService.GetBoards().Result;
    Queue<NavigationUserBoard> queue = new();
    queue.Enqueue(root);
    while (queue.Count > 0)
    {
      NavigationUserBoard navigation = queue.Dequeue();
      if (navigation is NavigationUserGroup navigationGroup)
      {
        var children = boards.Where(dto => dto.Parent == navigation.Id.Value);
        Guid previous = Guid.Empty;
        BoardDbDto? child;
        while ((child = children.FirstOrDefault(dto => dto.Previous == previous)) is not null)
        {
          NavigationUserBoard newNavigation;
          newNavigation = child.Grouped
            ? new NavigationUserGroup(child.Name, IconManager.ToIcon((IconType)child.IconType, child.IconValue), new BoardId(child.Id))
            : new NavigationUserBoard(child.Name, IconManager.ToIcon((IconType)child.IconType, child.IconValue), new BoardId(child.Id));
          navigationGroup.AddChild(newNavigation);
          queue.Enqueue(newNavigation);
          previous = child.Id;
        }
      }
    }
  }

  // 사용자 보드, 그룹 데이터베이스에 추가
  public void AddBoard(NavigationUserBoard board)
  {
    var icon = IconManager.ToIconTypeValue(board.Icon);
    InsertBoardDbDto dto = new()
    {
      Id = board.Id.Value,
      Grouped = board is NavigationUserGroup,
      Parent = board.Parent!.Id.Value,
      Previous = board.GetPrevious()?.Id.Value,
      Next = board.GetNext()?.Id.Value,
      Name = board.Name,
      IconType = icon.IconType,
      IconValue = icon.IconValue
    };
    _databaseService.AddBoard(dto);
  }

  // 데이터베이스에서 사용자 보드, 그룹 제거
  public void DeleteBoard(NavigationUserBoard board)
  {
    DeleteBoardDbDto dto = new() { Id = board.Id.Value };
    _databaseService.DeleteBoard(dto);
  }

  // 사용자 보드, 그룹 속성 변경 시 데이터베이스에 업데이트
  public void UpdateBoard(NavigationUserBoard board, BoardUpdateFields updateFields)
  {
    if (updateFields == BoardUpdateFields.None)
      return;

    var icon = IconManager.ToIconTypeValue(board.Icon);
    UpdateBoardDbDto dto = new()
    {
      UpdateFields = updateFields,
      Id = board.Id.Value,
      Parent = updateFields.HasFlag(BoardUpdateFields.Parent) ? board.Parent?.Id.Value : null,
      Previous = updateFields.HasFlag(BoardUpdateFields.Previous) ? board.GetPrevious()?.Id.Value : null,
      Name = updateFields.HasFlag(BoardUpdateFields.Name) ? board.Name : null,
      IconType = updateFields.HasFlag(BoardUpdateFields.IconType) ? icon.IconType : null,
      IconValue = updateFields.HasFlag(BoardUpdateFields.IconValue) ? icon.IconValue : null,
    };
    _databaseService.UpdateBoard(dto);
  }
  #endregion

  #region MainPage의 NavigationView와 Frame 등록 및 해제
  // MainPage 로드, 언로드 시 NavigationView, Frame 컨트롤을 반드시 등록 또는 해제해야 함
  private NavigationView? _navigationView;
  private Frame? _frame;

  public void AttachView(NavigationView navigationView, Frame frame)
  {
    _navigationView = navigationView;
    _frame = frame;
    _frame.Navigated += OnNavigated;
  }

  public void DetachView()
  {
    if (_frame is not null)
      _frame.Navigated -= OnNavigated;

    _navigationView = null;
    _frame = null;
  }
  #endregion

  #region Navigation 로직(현재 내비게이션, 페이지 이동 등)
  public NavigationItem? CurrentNavigation { get; private set; }
  public void SetCurrentNavigation(NavigationItem? navigation) => CurrentNavigation = navigation;

  private bool _preventNavigation = false;

  // 탐색, 페이지 이동 (파라미터로 Navigation 인스턴스 전달)
  public void Navigate(NavigationItem navigation)
  {
    if (_frame is null || _preventNavigation)
      return;
    _frame.Navigate(navigation.PageType, navigation);
    ReferenceTracker.NavigationPageReferences.Add(new(navigation.PageType.Name, (Page)_frame.Content));
  }

  // 뒤로 탐색
  public void GoBack()
  {
    if (_frame is null)
      return;

    while (_frame.CanGoBack)
    {
      var backStack = _frame.BackStack;

      if (backStack[^1].Parameter is NavigationUserBoard userBoard)
      {
        // 삭제된 Navigation인지 확인 후 삭제되었으면 이전 탐색으로 건너뜀
        if (!WeakReferenceMessenger.Default.Send(new ExtendedRequestMessage<NavigationUserBoard, bool>(userBoard), Tokens.IsValidNavigation).Response)
        {
          backStack.RemoveAt(backStack.Count - 1);
          continue;
        }
      }

      _frame.GoBack();
      break;
    }
  }

  // NavigationView.SelectionChanged 이벤트로 Navigate 메서드를 호출하여 페이지를 이동하므로
  // 페이지 이동 없이 NavigationView의 Selection을 변경하고자 할 때 사용
  public void ChangeNavigationViewSelectionWithoutNavigation(NavigationItem? navigation)
  {
    if (_navigationView is null)
      return;

    _preventNavigation = true;
    _navigationView.SelectedItem = navigation;
    _preventNavigation = false;
  }

  // 페이지 이동 후 CurrentNavigation을 해당 페이지로 변경
  private void OnNavigated(object sender, NavigationEventArgs e)
  {
    NavigationItem navigation = (NavigationItem)e.Parameter;
    CurrentNavigation = navigation;
    ChangeNavigationViewSelectionWithoutNavigation(navigation is NavigationSearch ? null : navigation);
  }

  // 페이지 컨텐트를 비활성화
  public void ToggleNavigationViewContentEnabled(bool enabled)
  {
    if (_frame is null)
      return;
    _frame.IsEnabled = enabled;
  }
  #endregion
}
