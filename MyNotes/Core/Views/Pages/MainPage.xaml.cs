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

    _timer.Tick += (s, e) =>
    {
      _timer.Stop();
      if (_hoveredGroupNavigationViewItem != null)
        _hoveredGroupNavigationViewItem.IsExpanded = _hoverGroupNavigationViewItemIsExpanded;
    };
  }

  public MainViewModel ViewModel { get; }

  private void VIEW_TitleBar_BackRequested(TitleBar sender, object args)
    => VIEW_NavigationContent_RootFrame.GoBack();

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
    VIEW_NavigationView.SelectedItem = (navigation is NavigationSearchItem) ? null : navigation;
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

  private void VIEW_NavigationViewFooter_EditNavigationViewItem_Click(object sender, RoutedEventArgs e)
  {
    VIEW_NavigationViewFooter_NewGroupButton.IsEnabled = !VIEW_NavigationViewFooter_NewGroupButton.IsEnabled;
    VIEW_NavigationViewFooter_NewBoardButton.IsEnabled = !VIEW_NavigationViewFooter_NewBoardButton.IsEnabled;
    foreach (NavigationBoardItem item in ViewModel.ListMenuItems)
      ViewModel.Recursive(item, x => x.IsEditMode = !x.IsEditMode);
  }

  private NavigationBoardItem? _sourceItem;
  private bool _isExpandedState;
  private async void Grid_DragStarting(UIElement sender, DragStartingEventArgs args)
  {
    Grid item = (Grid)sender;
    NavigationViewItem parentItem = (NavigationViewItem)item.Parent;
    _isExpandedState = parentItem.IsExpanded;
    _sourceItem = (NavigationBoardItem)parentItem.DataContext;
    parentItem.IsExpanded = false;

    var defferal = args.GetDeferral();
    var renderTargetBitmap = new RenderTargetBitmap();
    await renderTargetBitmap.RenderAsync(parentItem);

    var pixels = await renderTargetBitmap.GetPixelsAsync();

    var softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, renderTargetBitmap.PixelWidth, renderTargetBitmap.PixelHeight, BitmapAlphaMode.Premultiplied);
    softwareBitmap.CopyFromBuffer(pixels);
    Point position = item.TransformToVisual(parentItem).TransformPoint(new Point(0, 0));
    args.DragUI.SetContentFromSoftwareBitmap(softwareBitmap, position);
    defferal.Complete();

    parentItem.IsEnabled = false;
    //parentItem.Foreground = (SolidColorBrush)((App)Application.Current).Resources["TextFillColorDisabledBrush"];
  }

  private void Grid_DropCompleted(UIElement sender, DropCompletedEventArgs args)
  {
    Grid item = (Grid)sender;
    NavigationViewItem parentItem = (NavigationViewItem)item.Parent;
    if (parentItem is not null)
    {
      parentItem.IsExpanded = _isExpandedState;
      parentItem.IsEnabled = true;
    }
    //parentItem.Foreground = (SolidColorBrush)((App)Application.Current).Resources["TextFillColorPrimaryBrush"];
  }

  private const double ItemUpperPosition = 10.0;
  private const double ItemLowerPostion = 25.0;

  private DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(100) };
  private NavigationViewItem? _hoveredGroupNavigationViewItem;
  bool _hoverGroupNavigationViewItemIsExpanded;
  private void GroupNavigationViewItem_DragOver(object sender, DragEventArgs e)
  {
    NavigationViewItem item = (NavigationViewItem)sender;
    NavigationBoardGroupItem targetItem = (NavigationBoardGroupItem)item.DataContext;
    e.AcceptedOperation = DataPackageOperation.Move;


    double height = e.GetPosition(item).Y;
    _hoveredGroupNavigationViewItem = item;
    if (height <= ItemUpperPosition)
    {
      StartTimer(false);
      ViewModel.MoveNavigationBoardItem(_sourceItem!, targetItem, NavigationBoardItemPosition.Before);
    }
    else if (height >= ItemLowerPostion)
    {
      if (_sourceItem != targetItem)
        StartTimer(false);
      ViewModel.MoveNavigationBoardItem(_sourceItem!, targetItem, NavigationBoardItemPosition.After);
    }
    else if (_sourceItem != targetItem)
    {
      StartTimer(true);
      if (targetItem.Children.Count == 0)
        targetItem.Insert(0, _sourceItem!);
    }

    e.Handled = true;
  }

  private void StartTimer(bool isExpanded)
  {
    _hoverGroupNavigationViewItemIsExpanded = isExpanded;
    _timer.Start();
  }

  private void ItemNavigationViewItem_DragOver(object sender, DragEventArgs e)
  {
    NavigationViewItem item = (NavigationViewItem)sender;
    NavigationBoardItem targetItem = (NavigationBoardItem)item.DataContext;
    e.AcceptedOperation = DataPackageOperation.Move;

    if (_sourceItem != targetItem)
    {
      double height = e.GetPosition(item).Y;
      if (height <= ItemUpperPosition)
        ViewModel.MoveNavigationBoardItem(_sourceItem!, targetItem, NavigationBoardItemPosition.Before);
      else if (height >= ItemLowerPostion && targetItem.Parent!.Children.IndexOf(targetItem) >= targetItem.Parent.Children.Count - 2)
        ViewModel.MoveNavigationBoardItem(_sourceItem!, targetItem.Parent, NavigationBoardItemPosition.After);
      else
        ViewModel.MoveNavigationBoardItem(_sourceItem!, targetItem, NavigationBoardItemPosition.After);
    }

    e.Handled = true;
  }

  private void NavigationViewItem_DragLeave(object sender, DragEventArgs e)
  {
    NavigationViewItem item = (NavigationViewItem)sender;
    _timer.Stop();
  }
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