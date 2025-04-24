namespace MyNotes.Core.Views;

public sealed partial class MainWindow : Window
{
  public MainWindow()
  {
    this.InitializeComponent();
    this.ExtendsContentIntoTitleBar = true;
    OverlappedPresenter presenter = (OverlappedPresenter)AppWindow.Presenter;
    double dpi = 1.25;
    presenter.PreferredMinimumWidth = (int)(490 * dpi);
    presenter.PreferredMinimumHeight = (int)(800 * dpi);
  }
}