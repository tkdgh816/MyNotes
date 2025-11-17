using MyNotes.Common.Interop;
using MyNotes.Widget;

namespace MyNotes;

public class Program
{
  [STAThread]
  private static int Main(string[] args)
  {
    WinRT.ComWrappersSupport.InitializeComWrappers();

    // Windows 11 Widget
    if (IsWindowsVersion11OrHigher)
    {
      Guid CLSID_Factory = Guid.Parse("A5423B36-2D5C-45CA-9268-71B560D7269A");
      WidgetProviderFactory<WidgetProvider> widgetProviderFactory = new();
      WidgetProvider = widgetProviderFactory.Instance;
      _ = NativeMethods.CoRegisterClassObject(CLSID_Factory, widgetProviderFactory, 0x4, 0x1, out uint cookie);

      if (args.Contains("-Embedding", StringComparer.OrdinalIgnoreCase))
      {
        using (var emptyWidgetListEvent = WidgetProvider.EmptyWidgetListEvent)
        {
          emptyWidgetListEvent.WaitOne();
          WidgetProvider = null;
          _ = NativeMethods.CoRevokeClassObject(cookie);
        }
      }
      else
        LaunchAppSingleInstance();
    }
    else
      LaunchAppSingleInstance();


    return 0;
  }

  public static WidgetProvider? WidgetProvider { get; private set; }

  private static bool IsOSVersionAtLeast(int major, int minor, int build, int revision = 0)
  {
    ulong version = ulong.Parse(Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamilyVersion);
    var curMajor = (int)((version & 0xFFFF000000000000L) >> 48);
    var curMinor = (int)((version & 0x0000FFFF00000000L) >> 32);
    var curBuild = (int)((version & 0x00000000FFFF0000L) >> 16);
    var curRevision = (int)(version & 0x000000000000FFFFL);

    if (curMajor != major)
      return curMajor > major;

    if (curMinor != minor)
      return curMinor > minor;

    if (curBuild != build)
      return curBuild >= build;

    return curRevision >= revision;
  }

  public static bool IsWindowsVersion11OrHigher { get; } = IsOSVersionAtLeast(10, 0, 22000);

  private static void LaunchAppSingleInstance()
  {
    if (!DecideRedirection())
    {
      Application.Start((p) =>
      {
        var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
        SynchronizationContext.SetSynchronizationContext(context);
        _ = new App();
      });
    }
  }

  private static bool DecideRedirection()
  {
    bool isRedirect = false;
    AppActivationArguments args = AppInstance.GetCurrent().GetActivatedEventArgs();
    ExtendedActivationKind kind = args.Kind;
    AppInstance keyInstance = AppInstance.FindOrRegisterForKey("MyNotes");

    if (keyInstance.IsCurrent)
    {
      keyInstance.Activated += OnActivated;
    }
    else
    {
      isRedirect = true;
      RedirectActivationTo(args, keyInstance);
    }

    return isRedirect;
  }

  private static void OnActivated(object? sender, AppActivationArguments args)
  {
    ExtendedActivationKind kind = args.Kind;
  }

  public static IntPtr redirectEventHandle = IntPtr.Zero;

  // Do the redirection on another thread, and use a non-blocking
  // wait method to wait for the redirection to complete.
  public static void RedirectActivationTo(AppActivationArguments args, AppInstance keyInstance)
  {
    redirectEventHandle = NativeMethods.CreateEvent(IntPtr.Zero, true, false, null);
    Task.Run(() =>
    {
      keyInstance.RedirectActivationToAsync(args).AsTask().Wait();
      NativeMethods.SetEvent(redirectEventHandle);
    });

    uint CWMO_DEFAULT = 0;
    uint INFINITE = 0xFFFFFFFF;
    _ = NativeMethods.CoWaitForMultipleObjects(CWMO_DEFAULT, INFINITE, 1, [redirectEventHandle], out uint handleIndex);

    // Bring the window to the foreground
    Process process = Process.GetProcessById((int)keyInstance.ProcessId);
    NativeMethods.SetForegroundWindow(process.MainWindowHandle);
  }
}

