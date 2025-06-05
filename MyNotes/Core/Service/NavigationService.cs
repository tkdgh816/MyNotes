using System.Collections.Immutable;

using MyNotes.Common.Messaging;
using MyNotes.Core.Model;
using MyNotes.Debugging;

namespace MyNotes.Core.Service;

internal class NavigationService
{
  public NavigationService(DatabaseService databaseService) 
  {
    _databaseService = databaseService;
    _databaseService.BuildNavigationBoardTree(UserRootGroup);
    UserBoards = UserRootGroup.Children;
  }

  private void BuildNavigationTree()
  {
    Queue queue = new();
    queue.Enqueue(UserRootGroup);
    while(queue.Count > 0)
    {
      // "SELECT * FROM Boards WHERE parent = @parent AND next IS NULL"
      // "SELECT * FROM Boards WHERE parent = @parent AND next = @next";
    }
  }

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
    if(_frame is not null)
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
