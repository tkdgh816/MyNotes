using Windows.ApplicationModel;

namespace MyNotes.Core.View;

internal sealed partial class MainWindow : Window
{
  public MainWindow()
  {
    this.InitializeComponent();

    string iconPath = Path.Combine(Package.Current.InstalledLocation.Path, "Assets/icons/app/AppIcon_128.ico");
    AppWindow.SetIcon(iconPath);
  }
}