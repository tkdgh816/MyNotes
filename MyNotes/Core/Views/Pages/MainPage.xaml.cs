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
    //this.Loaded += MainPage_Loaded;
  }

  private void MainPage_Loaded(object sender, RoutedEventArgs e)
  {
    MenuFlyout ContextFlyout = new();
    ContextFlyout.Items.Add(new MenuFlyoutItem() { Text = "Context Flyout" });
    ScrollViewer MenuItemsHost = (ScrollViewer)((Grid)VisualTreeHelper.GetChild(VIEW_NavigationView, 0)).FindName("MenuItemsScrollViewer");
    MenuItemsHost.ContextFlyout = ContextFlyout;
  }

  public MainViewModel ViewModel { get; }

  private void VIEW_TitleBar_BackRequested(TitleBar sender, object args)
    => VIEW_NavigationContent_RootFrame.GoBack();

  private void VIEW_TitleBar_PaneToggleRequested(TitleBar sender, object args)
    => VIEW_NavigationView.IsPaneOpen = !VIEW_NavigationView.IsPaneOpen;

  private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
  {
    NavigationSearch searchItem = new(args.QueryText);
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
    {
      ViewModel.RegisterNavigation(item);
      VIEW_NavigationContent_RootFrame.Navigate(item.PageType, item);
    }
  }

  private void VIEW_NavigationContent_RootFrame_Navigated(object sender, NavigationEventArgs e)
  {
    _isBackRequested = true;
    NavigationItem navigation = (NavigationItem)e.Parameter;
    _currentNavigation = (navigation is NavigationSearch) ? null : navigation;
    VIEW_NavigationView.SelectedItem = _currentNavigation;
    _isBackRequested = false;
  }

  private async void ShowNewGroupContentDialog(int comboBoxIndex, object? selectedNavigation)
  {
    VIEW_NewBoardGroupInputTextBox.Text = "";
    ComboBox optionsComboBox = VIEW_NewBoardGroupOptionsComboBox;
    optionsComboBox.SelectedIndex = comboBoxIndex;

    if (await VIEW_NewBoardGroupContentDialog.ShowAsync() == ContentDialogResult.Primary)
    {
      NavigationBoardGroup group = optionsComboBox.SelectedIndex switch
      {
        1 => selectedNavigation as NavigationBoardGroup,
        2 => (selectedNavigation as NavigationBoard)?.Parent,
        0 or _ => ViewModel.BoardMenuRootItem
      } ?? ViewModel.BoardMenuRootItem;

      string inputText = VIEW_NewBoardGroupInputTextBox.Text;
      if (!string.IsNullOrWhiteSpace(inputText))
        ViewModel.AddNavigationBoardItem(group, inputText, true);
    }
  }

  private int GetNewBoardOptionsComboBoxSelectedIndex()
    => VIEW_NavigationView.SelectedItem switch
    {
      NavigationBoardGroup => 1,
      NavigationBoard => 2,
      _ => 0
    };

  private void VIEW_NavigationViewFooter_NewGroupButton_Click(object sender, RoutedEventArgs e)
    => ShowNewGroupContentDialog(GetNewBoardOptionsComboBoxSelectedIndex(), VIEW_NavigationView.SelectedItem);

  private async void ShowNewBoardContentDialog(int comboBoxIndex, object? selectedNavigation)
  {
    VIEW_NewBoardInputTextBox.Text = "";
    ComboBox optionsComboBox = VIEW_NewBoardOptionsComboBox;
    optionsComboBox.SelectedIndex = comboBoxIndex;

    if (await VIEW_NewBoardContentDialog.ShowAsync() == ContentDialogResult.Primary)
    {
      NavigationBoardGroup group = optionsComboBox.SelectedIndex switch
      {
        1 => selectedNavigation as NavigationBoardGroup,
        2 => (selectedNavigation as NavigationBoard)?.Parent,
        0 or _ => ViewModel.BoardMenuRootItem
      } ?? ViewModel.BoardMenuRootItem;

      string inputText = VIEW_NewBoardInputTextBox.Text;
      if (!string.IsNullOrWhiteSpace(inputText))
        ViewModel.AddNavigationBoardItem(group, inputText, false);
    }
  }
  private void VIEW_NavigationViewFooter_NewBoardButton_Click(object sender, RoutedEventArgs e)
    => ShowNewBoardContentDialog(GetNewBoardOptionsComboBoxSelectedIndex(), VIEW_NavigationView.SelectedItem);

  private void VIEW_NavigationViewBoardGroupItem_NewGroupMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    => ShowNewGroupContentDialog(1, ((FrameworkElement)sender).DataContext);

  private void VIEW_NavigationViewBoardGroupItem_NewBoardMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    => ShowNewBoardContentDialog(1, ((FrameworkElement)sender).DataContext);

  private async void ShowRenameBoardContentDialog(NavigationBoard navigation)
  {
    VIEW_RenameBoardInputTextBox.Text = navigation.Name;
    VIEW_RenameBoardContentDialog.DataContext = navigation;
    if (await VIEW_RenameBoardContentDialog.ShowAsync() == ContentDialogResult.Primary)
      ViewModel.RenameNavigation(navigation, VIEW_RenameBoardInputTextBox.Text);
  }

  private void VIEW_NavigationViewBoardItem_RenameMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    => ShowRenameBoardContentDialog((NavigationBoard)((FrameworkElement)sender).DataContext);

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
    foreach (NavigationBoard item in ViewModel.BoardMenuItems)
      ViewModel.Recursive(item, x => x.IsEditable = _isInEditMode);
    VIEW_NavigationView.SelectedItem = _isInEditMode ? null : _currentNavigation;

    if (!_isInEditMode)
      ViewModel.ResetNavigation();
    _isBackRequested = false;
  }

  private void VIEW_NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
  {
    if (args.InvokedItemContainer is NavigationViewItem itemContainer
        && !itemContainer.SelectsOnInvoked
        && itemContainer.DataContext is NavigationItem item)
    {
      switch (item)
      {
        case NavigationTags:
          VIEW_TagsFlyout.ShowAt(itemContainer);
          break;
        case NavigationBoardGroup:
          break;
        case NavigationBoard:
          VIEW_EditTeachingTip.IsOpen = true;
          break;
      }
    }
  }


  private void VIEW_NavigationView_Loaded(object sender, RoutedEventArgs e)
  {
    VIEW_NavigationView.SelectedItem = ViewModel.CoreMenuItems[0];
  }

  #region NavigationView MenuItems >> Drag and Drop 
  private NavigationBoard? _draggingSource;
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
    _draggingSource = (NavigationBoard)_draggingItem.DataContext;
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
    NavigationBoardGroup hoveredTarget = (NavigationBoardGroup)item.DataContext;
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
    NavigationBoard hoveredTarget = (NavigationBoard)item.DataContext;
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
      NavigationBoardGroup => BoardGroupMenuItemTemplate,
      NavigationBoard => BoardMenuItemTemplate,
      NavigationItem => MenuItemTemplate,
      NavigationSeparator => SeparatorTemplate,
      _ => throw new ArgumentException("")
    };
  }
}