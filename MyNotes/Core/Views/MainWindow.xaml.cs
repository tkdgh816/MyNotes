using MyNotes.Core.ViewModels;

namespace MyNotes.Views;

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

  private void VIEW_TitleBar_PaneToggleRequested(TitleBar sender, object args)
    => VIEW_NavigationView.IsPaneOpen = !VIEW_NavigationView.IsPaneOpen;

  private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
  {
    VIEW_NavigationView.SelectedItem = null;
  }
}
