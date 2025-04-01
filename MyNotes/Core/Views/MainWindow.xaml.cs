namespace MyNotes.Views;

public sealed partial class MainWindow : Window
{
  public MainWindow()
  {
    this.InitializeComponent();
    this.ExtendsContentIntoTitleBar = true;
    this.SetTitleBar(VIEW_TitleBar);
  }

  private void VIEW_TitleBar_PaneToggleRequested(TitleBar sender, object args)
    => VIEW_NavigationView.IsPaneOpen = !VIEW_NavigationView.IsPaneOpen;
}
