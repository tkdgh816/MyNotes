using Microsoft.UI.Xaml.Media.Imaging;

using MyNotes.Core.Models;
using MyNotes.Core.ViewModels;

using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;

namespace MyNotes.Core.Views;

public sealed partial class MainPage : Page
{
  public MainPage()
  {
    this.InitializeComponent();
    ViewModel = App.Current.GetService<MainViewModel>();
    SetTimer();
  }

  public MainViewModel ViewModel { get; }

  private void VIEW_TitleBar_BackRequested(TitleBar sender, object args)
    => VIEW_NavigationContent_RootFrame.GoBack();

  private void VIEW_TitleBar_PaneToggleRequested(TitleBar sender, object args)
    => VIEW_NavigationView.IsPaneOpen = !VIEW_NavigationView.IsPaneOpen;

  private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
  {
    NavigationSearchItem searchItem = new(args.QueryText);
    Navigate(searchItem);
  }

  bool _isBackRequested = false;
  private void VIEW_NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
  {
    if (args.SelectedItem is not NavigationItem selected)
      return;
    Navigate(selected);
  }

  private void Navigate(NavigationItem item)
  {
    if (!_isBackRequested)
      VIEW_NavigationContent_RootFrame.Navigate(item.PageType, item);
  }

  private void VIEW_NavigationContent_RootFrame_Navigated(object sender, NavigationEventArgs e)
  {
    _isBackRequested = true;
    NavigationItem navigation = (NavigationItem)e.Parameter;
    _currentNavigation = (navigation is NavigationSearchItem) ? null : navigation;
    VIEW_NavigationView.SelectedItem = _currentNavigation;
    _isBackRequested = false;
  }

  private void VIEW_NavigationViewFooter_NewGroupButton_Click(object sender, RoutedEventArgs e)
  {
    NavigationBoardGroupItem group = VIEW_NavigationView.SelectedItem switch
    {
      NavigationBoardGroupItem groupItem => groupItem,
      NavigationBoardItem boardItem => boardItem.Parent,
      _ => ViewModel.ListMenuRootItem
    };
    group.Add(new NavigationBoardGroupItem("New Group", new FontIconSource() { Glyph = "\uE82D" }, new Guid()));
  }

  private void VIEW_NavigationViewFooter_NewBoardButton_Click(object sender, RoutedEventArgs e)
  {
    NavigationBoardGroupItem group = VIEW_NavigationView.SelectedItem switch
    {
      NavigationBoardGroupItem groupItem => groupItem,
      NavigationBoardItem boardItem => boardItem.Parent,
      _ => ViewModel.ListMenuRootItem
    };
    group.Add(new NavigationBoardItem("New Board", new FontIconSource() { Glyph = "\uE737" }, new Guid()));
  }

  private bool _isInEditMode = false;
  private NavigationItem? _currentNavigation;
  private void VIEW_NavigationViewFooter_EditNavigationViewItem_Click(object sender, RoutedEventArgs e)
  {
    _isBackRequested = true;
    _isInEditMode = !_isInEditMode;

    VIEW_NavigationViewFooter_NewGroupButton.IsEnabled = !_isInEditMode;
    VIEW_NavigationViewFooter_NewBoardButton.IsEnabled = !_isInEditMode;

    VIEW_TitleBar.IsBackButtonVisible = !_isInEditMode;
    VIEW_NavigationVIew_AutoSuggestBox.IsEnabled = !_isInEditMode;
    foreach (Navigation item in ViewModel.CoreMenuItems)
      item.IsEditable = _isInEditMode;
    foreach (Navigation item in ViewModel.FooterMenuItems)
      item.IsEditable = _isInEditMode;
    foreach (NavigationBoardItem item in ViewModel.ListMenuItems)
      ViewModel.Recursive(item, x => x.IsEditable = _isInEditMode);
    VIEW_NavigationView.SelectedItem = _isInEditMode ? null : _currentNavigation;

    _isBackRequested = false;
  }

  private void VIEW_NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
  {
    if (args.InvokedItemContainer is NavigationViewItem item && item.DataContext is not NavigationBoardGroupItem && !item.SelectsOnInvoked)
      VIEW_EditTeachingTip.IsOpen = true;
  }

  private void VIEW_NavigationView_Loaded(object sender, RoutedEventArgs e)
  {
    VIEW_NavigationView.SelectedItem = ViewModel.CoreMenuItems[0];
  }

  #region NavigationView MenuItems >> Drag and Drop 
  private NavigationBoardItem? _draggingSource;
  private NavigationViewItem? _draggingItem;
  private bool _isDraggingItemExpanded;

  private NavigationViewItem? _hoveredGroup;
  bool _isInsertingIntoHoveredGroup;

  private const double HoveredItemUpperPosition = 10.0;
  private const double HoveredItemLowerPostion = 25.0;

  private readonly DispatcherTimer _timer = new();
  private const int TimerIntervalMilliseconds = 200;

  private async void NavigationViewItem_DragStarting(UIElement sender, DragStartingEventArgs args)
  {
    _draggingItem = (NavigationViewItem)sender;
    _isDraggingItemExpanded = _draggingItem.IsExpanded;
    _draggingSource = (NavigationBoardItem)_draggingItem.DataContext;
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
    NavigationBoardGroupItem hoveredTarget = (NavigationBoardGroupItem)item.DataContext;
    e.AcceptedOperation = DataPackageOperation.Move;

    double height = e.GetPosition(item).Y;
    _hoveredGroup = item;
    if (height <= HoveredItemUpperPosition)
    {
      StartTimer(false);
      ViewModel.MoveNavigationBoardItem(_draggingSource!, hoveredTarget, NavigationBoardItemPosition.Before);
    }
    else if (height >= HoveredItemLowerPostion)
    {
      if (_draggingSource != hoveredTarget)
        StartTimer(false);
      ViewModel.MoveNavigationBoardItem(_draggingSource!, hoveredTarget, NavigationBoardItemPosition.After);
    }
    else if (_draggingSource != hoveredTarget)
    {
      StartTimer(true);
      hoveredTarget.Insert(0, _draggingSource!);
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
    NavigationBoardItem hoveredTarget = (NavigationBoardItem)item.DataContext;
    e.AcceptedOperation = DataPackageOperation.Move;

    if (_draggingSource != hoveredTarget)
    {
      double height = e.GetPosition(item).Y;
      if (height <= HoveredItemUpperPosition)
        ViewModel.MoveNavigationBoardItem(_draggingSource!, hoveredTarget, NavigationBoardItemPosition.Before);
      else if (height >= HoveredItemLowerPostion && hoveredTarget.Parent!.Children.IndexOf(hoveredTarget) >= hoveredTarget.Parent.Children.Count - 2)
        ViewModel.MoveNavigationBoardItem(_draggingSource!, hoveredTarget.Parent, NavigationBoardItemPosition.After);
      else
        ViewModel.MoveNavigationBoardItem(_draggingSource!, hoveredTarget, NavigationBoardItemPosition.After);
    }

    e.Handled = true;
  }

  private void NavigationViewItem_DragLeave(object sender, DragEventArgs e)
  {
    _timer.Stop();
  }
  #endregion
}

public class MainNavigationViewMenuItemTemplateSelector : DataTemplateSelector
{
  public DataTemplate? MenuItemTemplate { get; set; }
  public DataTemplate? SeparatorTemplate { get; set; }
  public DataTemplate? BoardGroupMenuItemTemplate { get; set; }
  public DataTemplate? BoardMenuItemTemplate { get; set; }
  protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
  {
    return item switch
    {
      NavigationBoardGroupItem => BoardGroupMenuItemTemplate,
      NavigationBoardItem => BoardMenuItemTemplate,
      NavigationItem => MenuItemTemplate,
      NavigationSeparator => SeparatorTemplate,
      _ => throw new ArgumentException("")
    };
  }
}