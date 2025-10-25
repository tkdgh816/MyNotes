using System.Threading;

using Microsoft.UI.Dispatching;
using Microsoft.Windows.AppLifecycle;

using MyNotes.Common.Interop;

namespace MyNotes;

internal partial class Program
{
  [STAThread]
  private static int Main(string[] args)
  {
    WinRT.ComWrappersSupport.InitializeComWrappers();
    bool isRedirect = DecideRedirection();

    if (!isRedirect)
    {
      Application.Start((p) =>
      {
        var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
        SynchronizationContext.SetSynchronizationContext(context);
        _ = new App();
      });
    }

    return 0;
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
