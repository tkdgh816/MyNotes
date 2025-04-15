using MyNotes.Core.Models;
using MyNotes.Core.ViewModels;

namespace MyNotes.Core.Views;

public sealed partial class MainWindow : Window
{
  public MainWindow()
  {
    this.InitializeComponent();
    ViewModel = App.Current.GetService<MainViewModel>();

    this.ExtendsContentIntoTitleBar = true;
    this.SetTitleBar(VIEW_TitleBar);
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

  private void NavigationViewItem_Tapped(object sender, TappedRoutedEventArgs e)
  {
    FlyoutBase.ShowAttachedFlyout(VIEW_NavigationViewFooter_AddNavigationViewItem);
  }

  private void VIEW_NavigationContent_RootFrame_Navigated(object sender, NavigationEventArgs e)
  {
    _isBackRequested = true;
    NavigationItem navigation = (NavigationItem)e.Parameter;
    VIEW_NavigationView.SelectedItem = (navigation is NavigationSearchItem) ? null : navigation;
    _isBackRequested = false;
  }

  private void NavigationViewItem_DragEnter(object sender, DragEventArgs e)
  {
    Debug.WriteLine("Drag Enter");
  }

  private void VIEW_NavigationViewFooter_EditButton_Click(object sender, RoutedEventArgs e)
  {
    foreach(NavigationItem item in ViewModel.ListMenuItems)
      ViewModel.Recursive(item, x => x.IsMovable = !x.IsMovable);
  }
}

public class MainNavigationViewMenuItemTemplateSelector : DataTemplateSelector
{
  public DataTemplate? GroupItemTemplate { get; set; }
  public DataTemplate? MenuItemTemplate { get; set; }
  public DataTemplate? SeparatorTemplate { get; set; }
  protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
  {
    return item switch
    {
      NavigationGroupItem => GroupItemTemplate,
      NavigationItem => MenuItemTemplate,
      NavigationSeparator => SeparatorTemplate,
      _ => null
    };
  }
}