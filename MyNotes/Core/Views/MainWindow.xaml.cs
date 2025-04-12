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

  MainViewModel ViewModel { get; }

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