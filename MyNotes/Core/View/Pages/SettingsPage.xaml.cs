using MyNotes.Core.ViewModel;

using Windows.System;
using Windows.UI.Shell;

namespace MyNotes.Core.View;
internal sealed partial class SettingsPage : Page
{
  public SettingsViewModel ViewModel { get; }

  public SettingsPage()
  {
    this.InitializeComponent();
    ViewModel = App.Instance.GetService<SettingsViewModel>();
  }

  private async void RunAtStartupButton_Click(object sender, RoutedEventArgs e)
  {
    bool result = await Launcher.LaunchUriAsync(new Uri("shell:appsfolder"));
    //bool result = await Launcher.LaunchUriAsync(new Uri(@"https://www.google.com/"));
    Debug.WriteLine(result);
  }

  private async void PinToTaskbarButton_Click(object sender, RoutedEventArgs e)
  {
    bool isPinningAllowed = TaskbarManager.GetDefault().IsPinningAllowed;
    bool isPinned = await TaskbarManager.GetDefault().IsCurrentAppPinnedAsync();
    Debug.WriteLine($"Pinnig Allowed: {isPinningAllowed}, Pinned: {isPinned}");
    //if (!isPinned)
    //  await TaskbarManager.GetDefault().RequestPinCurrentAppAsync();
  }
}
