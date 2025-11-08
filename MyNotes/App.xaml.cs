namespace MyNotes;

public partial class App : Application
{
  private Window? _window;

  public App()
  {
    InitializeComponent();
  }

  protected override void OnLaunched(LaunchActivatedEventArgs args)
  {
    _window = new Views.Windows.MainWindow();
    _window.Activate();
  }
}
