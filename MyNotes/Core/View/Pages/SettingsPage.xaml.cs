using Microsoft.Windows.AppLifecycle;

using MyNotes.Core.Service;
using MyNotes.Core.ViewModel;

using Windows.ApplicationModel;
using Windows.Foundation.Metadata;
using Windows.UI.Shell;

namespace MyNotes.Core.View;
internal sealed partial class SettingsPage : Page
{
  public SettingsViewModel ViewModel { get; }

  public SettingsPage()
  {
    this.InitializeComponent();
    ViewModel = App.Instance.GetService<SettingsViewModel>();

    this.Unloaded += SettingsPage_Unloaded;
  }

  private void SettingsPage_Unloaded(object sender, RoutedEventArgs e) => this.Bindings.StopTracking();

  private async void RunAtStartupButton_Click(object sender, RoutedEventArgs e)
  {
    Debug.WriteLine("RunAtStartup");
    //bool result = await Launcher.LaunchUriAsync(new Uri("shell:appsfolder"));
    ////bool result = await Launcher.LaunchUriAsync(new Uri(@"https://www.google.com/"));
    //Debug.WriteLine(result);

    StartupTask startupTask = await StartupTask.GetAsync("MyNotesStartupId");
    switch (startupTask.State)
    {
      case StartupTaskState.Disabled:
        await startupTask.RequestEnableAsync();
        break;
      case StartupTaskState.DisabledByUser:
        // Task is disabled and user must enable it manually.
        ContentDialog dialog = new()
        {
          XamlRoot = (App.Instance.GetService<WindowService>().MainWindow?.Content as Page)?.XamlRoot,
          Content =
            """
            You have disabled this app's ability to run as soon as you sign in,
            but if you change your mind, you can enable this in the Startup tab in Task Manager.
            """,
          Title = "TestStartup"
        };
        await dialog.ShowAsync();
        break;
      case StartupTaskState.DisabledByPolicy:
        Debug.WriteLine("Startup disabled by group policy, or not supported on this device");
        break;
      case StartupTaskState.Enabled:
        startupTask.Disable();
        break;
    }
    Debug.WriteLine($"result = {startupTask.State}");
  }

  private async void PinToTaskbarButton_Click(object sender, RoutedEventArgs e)
  {
    if (ApiInformation.IsTypePresent("Windows.UI.Shell.TaskbarManager"))
    {
      var lafRes = LimitedAccessFeatures.TryUnlockFeature("com.microsoft.windows.taskbar.pin", "", "");
      Debug.WriteLine(lafRes.Status);

      bool isPinningAllowed = TaskbarManager.GetDefault().IsPinningAllowed;
      Debug.WriteLine("isPinningAllowed: {0}", isPinningAllowed);

      if (isPinningAllowed)
      {
        bool isPinned = await TaskbarManager.GetDefault().IsCurrentAppPinnedAsync();
        Debug.WriteLine("isPinned: {0}", isPinned);

        var licenseInfo = Windows.ApplicationModel.Store.CurrentApp.LicenseInformation;

        Debug.WriteLine("IsActive: {0}", licenseInfo.IsActive);
        Debug.WriteLine("IsTrial: {0}", licenseInfo.IsTrial);

        if (!isPinned && licenseInfo.IsActive && !licenseInfo.IsTrial)
        {
          await TaskbarManager.GetDefault().RequestPinCurrentAppAsync();
        }
      }
    }
  }

  private void View_LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    if (ViewModel.AppLanguage == ViewModel.InitialAppLanguage)
      VisualStateManager.GoToState(this, "LanguageNotChanged", false);
    else
      VisualStateManager.GoToState(this, "LanguageChanged", false);
  }

  private void RelaunchButton_Click(object sender, RoutedEventArgs e)
  {
    Microsoft.Windows.AppLifecycle.AppInstance.Restart("");
  }
}
