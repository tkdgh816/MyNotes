using System.Collections.Immutable;

using MyNotes.Common.Messaging;
using MyNotes.Core.Dto;
using MyNotes.Core.Model;
using MyNotes.Debugging;

namespace MyNotes.Core.Service;

internal class NavigationService
{
  public NavigationService(DatabaseService databaseService)
  {
    _databaseService = databaseService;
    //BuildNavigationTree();
    UserBoards = UserRootGroup.Children;
  }

  public void AddBoard(NavigationUserBoard board)
  {
    var icon = GetIconTypeAndValue(board.Icon);
    BoardDbInsertDto dto = new() { Id = board.Id, Grouped = board is NavigationUserGroup, Parent = board.Parent!.Id, Previous = board.GetPrevious()?.Id, Next = board.GetNext()?.Id, Name = board.Name, IconType = icon.IconType, IconValue = icon.IconValue };
    _databaseService.AddBoard(dto);
  }

  public void DeleteBoard(NavigationUserBoard board)
  {
    BoardDbDeleteDto dto = new() { Id = board.Id };
    _databaseService.DeleteBoard(dto);
  }

  public void UpdateBoard(NavigationUserBoard board, BoardUpdateFields updateFields)
  {
    if (updateFields == BoardUpdateFields.None)
      return;

    var icon = GetIconTypeAndValue(board.Icon);
    BoardDbUpdateDto dto = new()
    {
      UpdateFields = updateFields,
      Id = board.Id,
      Parent = updateFields.HasFlag(BoardUpdateFields.Parent) ? board.Parent?.Id : null,
      Previous = updateFields.HasFlag(BoardUpdateFields.Previous) ? board.GetPrevious()?.Id : null,
      Name = updateFields.HasFlag(BoardUpdateFields.Name) ? board.Name : null,
      IconType = updateFields.HasFlag(BoardUpdateFields.IconType) ? icon.IconType : null,
      IconValue = updateFields.HasFlag(BoardUpdateFields.IconValue) ? icon.IconValue : null,
    };
    _databaseService.UpdateBoard(dto);
  }

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
        var children = boards.Where(dto => dto.Parent == navigation.Id);
        Guid previous = Guid.Empty;
        BoardDbDto? child;
        while ((child = children.FirstOrDefault(dto => dto.Previous == previous)) is not null)
        {
          NavigationUserBoard newNavigation;
          newNavigation = child.Grouped
            ? new NavigationUserGroup(child.Name, GetIcon((IconType)child.IconType, child.IconValue), child.Id)
            : new NavigationUserBoard(child.Name, GetIcon((IconType)child.IconType, child.IconValue), child.Id);
          navigationGroup.AddChild(newNavigation);
          queue.Enqueue(newNavigation);
          previous = child.Id;
        }
      }
    }
  }

  private Icon GetIcon(IconType iconType, int iconValue)
  => iconType switch
  {
    IconType.Glyph => IconLibrary.FindGlyph(iconValue),
    IconType.Emoji => IconLibrary.FindEmoji(iconValue),
    _ => IconLibrary.FindEmoji(0)
  };

  private (int IconType, int IconValue) GetIconTypeAndValue(Icon icon)
  => ((int)icon.IconType, icon.IconType switch
  {
    IconType.Glyph => char.ConvertToUtf32(icon.Code, 0),
    IconType.Emoji => ((Emoji)icon).Index,
    _ => 0
  });

  private readonly DatabaseService _databaseService;

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

  private NavigationView? _navigationView;
  private Frame? _frame;

  public NavigationItem? CurrentNavigation { get; set; }
  private bool _preventNavigation = false;

  public void Navigate(NavigationItem navigation)
  {
    if (_frame is null)
      return;

    if (!_preventNavigation)
    {
      _frame.Navigate(navigation.PageType, navigation);
      //TEST: Page
      ReferenceTracker.NavigationPageReferences.Add(new((Page)_frame.Content));
    }
  }

  public void GoBack()
  {
    if (_frame is null)
      return;

    while (_frame.CanGoBack)
    {
      var backStack = _frame.BackStack;

      if (backStack[^1].Parameter is NavigationUserBoard userBoard)
      {
        if (!WeakReferenceMessenger.Default.Send(new ExtendedRequestMessage<bool>(userBoard), Tokens.IsValidNavigation).Response)
        {
          backStack.RemoveAt(backStack.Count - 1);
          continue;
        }
      }

      _frame.GoBack();
      break;
    }
  }

  public void ChangeNavigationViewSelectionWithoutNavigation(NavigationItem? navigation)
  {
    if (_navigationView is null)
      return;

    _preventNavigation = true;
    _navigationView.SelectedItem = navigation;
    _preventNavigation = false;
  }

  private void OnNavigated(object sender, NavigationEventArgs e)
  {
    NavigationItem navigation = (NavigationItem)e.Parameter;
    CurrentNavigation = navigation;
    ChangeNavigationViewSelectionWithoutNavigation(navigation is NavigationSearch ? null : navigation);
  }
}
