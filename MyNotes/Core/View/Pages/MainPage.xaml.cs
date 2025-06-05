using Microsoft.UI.Xaml.Media.Imaging;

using MyNotes.Common.Messaging;
using MyNotes.Core.Model;
using MyNotes.Core.Service;
using MyNotes.Core.ViewModel;
using MyNotes.Debugging;

using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;

namespace MyNotes.Core.View;

internal sealed partial class MainPage : Page
{
  public MainPage()
  {
    this.InitializeComponent();
    ViewModel = App.Current.GetService<MainViewModel>();
    var navigationService = App.Current.GetService<NavigationService>();
    navigationService.AttachView(View_NavigationView, View_NavigationContent_RootFrame);
    SetTimer();
    RegisterMessengers();
  }

  public MainViewModel ViewModel { get; }

  private void View_TitleBar_PaneToggleRequested(TitleBar sender, object args)
    => View_NavigationView.IsPaneOpen = !View_NavigationView.IsPaneOpen;

  #region Navigation
  private void View_NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
  {
    if (args.SelectedItem is NavigationItem navigation)
      ViewModel.NavigateCommand?.Execute(navigation);
  }
  #endregion

  #region Rename Board
  private void Template_Board_RenameMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    => ShowRenameBoardContentDialog((NavigationUserBoard)((FrameworkElement)sender).DataContext);

  private async void ShowRenameBoardContentDialog(NavigationUserBoard navigation)
  {
    View_RenameBoardInputTextBox.Text = navigation.Name;
    View_RenameBoardContentDialog.DataContext = navigation;
    if (await View_RenameBoardContentDialog.ShowAsync() == ContentDialogResult.Primary)
      ViewModel.RenameNavigationBoardMenu(navigation, View_RenameBoardInputTextBox.Text);
  }
  #endregion

  #region Add New Board
  // NavigationView Buttons
  private void View_NavigationViewFooter_NewBoardButton_Click(object sender, RoutedEventArgs e)
    => ShowNewBoardDialog(ViewModel.UserRootGroup);

  private void View_NavigationViewFooter_NewGroupButton_Click(object sender, RoutedEventArgs e)
    => ShowNewGroupDialog(ViewModel.UserRootGroup);

  // Context Flyout Buttons
  private void Template_Group_NewGroupMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    => ShowNewGroupDialog((NavigationUserGroup)((FrameworkElement)sender).DataContext);

  private void Template_Group_NewBoardMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    => ShowNewBoardDialog((NavigationUserGroup)((FrameworkElement)sender).DataContext);

  private async void ShowNewGroupDialog(NavigationUserGroup group)
  {
    ViewModel.NewBoardGroupName = "";
    ViewModel.NavigationGroupToAdd = group;
    Debug.WriteLine(group.Name);
    await View_NewGroupContentDialog.ShowAsync();
  }

  private async void ShowNewBoardDialog(NavigationUserGroup group)
  {
    ViewModel.NewBoardName = "";
    ViewModel.NavigationGroupToAdd = group;
    Debug.WriteLine(group.Name);
    await View_NewBoardContentDialog.ShowAsync();
  }
  #endregion

  #region Remove Board
  private void Template_Board_RemoveMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    => ShowRemoveBoardContentDialog((NavigationUserBoard)((FrameworkElement)sender).DataContext);

  private async void ShowRemoveBoardContentDialog(NavigationUserBoard navigation)
  {
    if (await View_RemoveBoardContentDialog.ShowAsync() == ContentDialogResult.Primary)
      ViewModel.RemoveNavigationBoardMenu(navigation);
  }
  #endregion

  private void View_NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
  {
    if (args.InvokedItemContainer is NavigationViewItem itemContainer
        && !itemContainer.SelectsOnInvoked
        && itemContainer.DataContext is NavigationItem item)
    {
      switch (item)
      {
        case NavigationTags:
          View_TagsFlyout.ShowAt(itemContainer);
          break;
        case NavigationUserGroup:
          break;
        case NavigationUserBoard:
          View_EditTeachingTip.IsOpen = true;
          break;
      }
    }
  }

  #region NavigationView MenuItems >> Drag and Drop 
  private NavigationUserBoard? _draggingSource;
  private NavigationViewItem? _draggingItem;
  private bool _isDraggingItemExpanded;

  private NavigationViewItem? _hoveredGroup;
  private bool _isInsertingIntoHoveredGroup;

  private const double HoveredItemUpperPosition = 10.0;
  private const double HoveredItemLowerPostion = 25.0;

  private readonly DispatcherTimer _timer = new();
  private const int TimerIntervalMilliseconds = 200;

  private async void NavigationViewItem_DragStarting(UIElement sender, DragStartingEventArgs args)
  {
    _draggingItem = (NavigationViewItem)sender;
    _isDraggingItemExpanded = _draggingItem.IsExpanded;
    _draggingSource = (NavigationUserBoard)_draggingItem.DataContext;
    _draggingItem.IsExpanded = false;
    _draggingItem.IsEnabled = false;

    DragOperationDeferral defferal = args.GetDeferral();
    RenderTargetBitmap renderTargetBitmap = new();
    await renderTargetBitmap.RenderAsync(_draggingItem);

    var pixels = await renderTargetBitmap.GetPixelsAsync();

    SoftwareBitmap softwareBitmap = new(BitmapPixelFormat.Bgra8, renderTargetBitmap.PixelWidth, renderTargetBitmap.PixelHeight, BitmapAlphaMode.Premultiplied);
    softwareBitmap.CopyFromBuffer(pixels);
    args.DragUI.SetContentFromSoftwareBitmap(softwareBitmap, new Point(0, 0));
    defferal.Complete();
  }

  private void NavigationViewItem_DropCompleted(UIElement sender, DropCompletedEventArgs args)
  {
    if (_hoveredGroup is not null && _isInsertingIntoHoveredGroup && !_hoveredGroup.IsExpanded)
    {
      _hoveredGroup.LayoutUpdated += hoveredGroup_LayoutUpdated;
      _hoveredGroup.IsExpanded = true;
    }
    else
    {
      RevertDraggedNavigationViewItemUI();
      _hoveredGroup = null;
    }
  }

  private void hoveredGroup_LayoutUpdated(object? sender, object e)
  {
    RevertDraggedNavigationViewItemUI();
    _hoveredGroup!.LayoutUpdated -= hoveredGroup_LayoutUpdated;
    _hoveredGroup = null;
  }

  private void RevertDraggedNavigationViewItemUI()
  {
    if (_draggingItem is not null)
    {
      _draggingItem.IsExpanded = _isDraggingItemExpanded;
      _draggingItem.IsEnabled = true;
      _draggingItem = null;
    }
  }

  private void GroupNavigationViewItem_DragOver(object sender, DragEventArgs e)
  {
    NavigationViewItem item = (NavigationViewItem)sender;
    NavigationUserGroup hoveredTarget = (NavigationUserGroup)item.DataContext;
    e.AcceptedOperation = DataPackageOperation.Move;

    double height = e.GetPosition(item).Y;
    _hoveredGroup = item;
    if (height <= HoveredItemUpperPosition)
    {
      StartTimer(false);
      ViewModel.MoveNavigationBoardMenu(_draggingSource!, hoveredTarget, NavigationBoardItemPosition.Before);
    }
    else if (height >= HoveredItemLowerPostion)
    {
      if (_draggingSource != hoveredTarget)
        StartTimer(false);
      ViewModel.MoveNavigationBoardMenu(_draggingSource!, hoveredTarget, NavigationBoardItemPosition.After);
    }
    else if (_draggingSource != hoveredTarget)
    {
      StartTimer(true);
      hoveredTarget.InsertChild(0, _draggingSource!);
    }

    e.Handled = true;
  }

  private void SetTimer()
  {
    _timer.Interval = TimeSpan.FromMilliseconds(TimerIntervalMilliseconds);
    _timer.Tick += (s, e) =>
    {
      _timer.Stop();
      if (_hoveredGroup != null)
        _hoveredGroup.IsExpanded = _isInsertingIntoHoveredGroup;
    };
  }

  private void StartTimer(bool isExpanded)
  {
    _isInsertingIntoHoveredGroup = isExpanded;
    _timer.Start();
  }

  private void ItemNavigationViewItem_DragOver(object sender, DragEventArgs e)
  {
    NavigationViewItem item = (NavigationViewItem)sender;
    NavigationUserBoard hoveredTarget = (NavigationUserBoard)item.DataContext;
    e.AcceptedOperation = DataPackageOperation.Move;

    if (_draggingSource != hoveredTarget)
    {
      double height = e.GetPosition(item).Y;
      if (height <= HoveredItemUpperPosition)
        ViewModel.MoveNavigationBoardMenu(_draggingSource!, hoveredTarget, NavigationBoardItemPosition.Before);
      else if (height >= HoveredItemLowerPostion && hoveredTarget.Parent!.Children.IndexOf(hoveredTarget) >= hoveredTarget.Parent.Children.Count - 2)
        ViewModel.MoveNavigationBoardMenu(_draggingSource!, hoveredTarget.Parent, NavigationBoardItemPosition.After);
      else
        ViewModel.MoveNavigationBoardMenu(_draggingSource!, hoveredTarget, NavigationBoardItemPosition.After);
    }

    e.Handled = true;
  }

  private void NavigationViewItem_DragLeave(object sender, DragEventArgs e)
  {
    _timer.Stop();
  }
  #endregion

  #region Messengers
  private void RegisterMessengers()
  {
    WeakReferenceMessenger.Default.Register<Message, string>(this, Tokens.RenameBoardName, new((recipient, message) => ShowRenameBoardContentDialog((NavigationUserBoard)message.Sender!)));
    WeakReferenceMessenger.Default.Register<Message, string>(this, Tokens.RemoveBoard, new((recipient, message) => ShowRemoveBoardContentDialog((NavigationUserBoard)message.Sender!)));

    WeakReferenceMessenger.Default.Register<AsyncRequestMessage<string>, string>(this, Tokens.RenameNoteTitle, new((recipient, message) =>
    {
      async Task<string> t()
      {
        if (await View_RenameNoteTitleContentDialog.ShowAsync() == ContentDialogResult.Primary)
          return ViewModel.NoteTitleToRename;
        else
          return "";
      }
      message.Reply(t());
    }));
  }
  #endregion

  //TEST: View Windows And Pages
  private void ViewWindowsButton_Click(object sender, RoutedEventArgs e) => ReferenceTracker.ReadActiveWindows();

  //TEST: ContextFlyout
  private void MainPage_Loaded(object sender, RoutedEventArgs e)
  {
    MenuFlyout ContextFlyout = new();
    ContextFlyout.Items.Add(new MenuFlyoutItem() { Text = "Context Flyout" });
    ScrollViewer MenuItemsHost = (ScrollViewer)((Grid)VisualTreeHelper.GetChild(View_NavigationView, 0)).FindName("MenuItemsScrollViewer");
    MenuItemsHost.ContextFlyout = ContextFlyout;
  }
}

internal class MainNavigationViewMenuItemTemplateSelector : DataTemplateSelector
{
  public DataTemplate? MenuItemTemplate { get; set; }
  public DataTemplate? SeparatorTemplate { get; set; }
  public DataTemplate? UserGroupMenuItemTemplate { get; set; }
  public DataTemplate? UserBoardMenuItemTemplate { get; set; }
  protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container) => item switch
  {
    NavigationUserGroup => UserGroupMenuItemTemplate,
    NavigationUserBoard => UserBoardMenuItemTemplate,
    NavigationItem => MenuItemTemplate,
    NavigationSeparator => SeparatorTemplate,
    _ => throw new ArgumentException("")
  };
}