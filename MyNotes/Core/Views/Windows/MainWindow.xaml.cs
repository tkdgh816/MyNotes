using MyNotes.Core.Services;

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

    this.Closed += MainWindow_Closed;
  }

  private void MainWindow_Closed(object sender, WindowEventArgs args)
  {
    App.Current.GetService<WindowService>().DestroyMainWindow();
  }
}