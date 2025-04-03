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
  {
    if (ViewModel.IsBackEnabled = ViewModel.PreviousNavigations.Count > 0)
    {
      ViewModel.IsBackRequested = true;
      VIEW_NavigationView.SelectedItem = ViewModel.PreviousNavigations.Pop();
      ViewModel.IsBackRequested = false;
    }
  }

  private void VIEW_TitleBar_PaneToggleRequested(TitleBar sender, object args)
    => VIEW_NavigationView.IsPaneOpen = !VIEW_NavigationView.IsPaneOpen;

  private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
  {
    VIEW_NavigationContent_RootFrame.Navigate(typeof(TestPage), "Search");
    if (ViewModel.CurrentNavigation is not null)
    {
      ViewModel.PreviousNavigations.Push(ViewModel.CurrentNavigation);
      ViewModel.CurrentNavigation = null;
      VIEW_NavigationView.SelectedItem = null;
    }
  }

  private void VIEW_NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
  {
    if (args.SelectedItem is not NavigationItem selected)
      return;

    VIEW_NavigationContent_RootFrame.Navigate(selected.PageType, selected?.Name);
    if (ViewModel.CurrentNavigation is not null && !ViewModel.IsBackRequested)
      ViewModel.PreviousNavigations.Push(ViewModel.CurrentNavigation);
    ViewModel.CurrentNavigation = selected;
    ViewModel.IsBackEnabled = ViewModel.PreviousNavigations.Count > 0;
  }
}

public class MainNavigationViewMenuItemTemplateSelector : DataTemplateSelector
{
  public DataTemplate? ItemTemplate { get; set; }
  public DataTemplate? SeparatorTemplate { get; set; }
  protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
  {
    Debug.WriteLine(item.GetType());
    return item switch
    {
      NavigationItem => ItemTemplate,
      NavigationSeparator => SeparatorTemplate,
      _ => null
    };
  }
}