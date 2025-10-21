using System.Runtime.InteropServices;
using System.Threading;

using Microsoft.UI.Dispatching;
using Microsoft.Windows.AppLifecycle;

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
        var context = new DispatcherQueueSynchronizationContext(
            DispatcherQueue.GetForCurrentThread());
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
    AppInstance keyInstance = AppInstance.FindOrRegisterForKey("MySingleInstanceApp");

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

  [LibraryImport("kernel32.dll", EntryPoint = "CreateEventW", StringMarshalling = StringMarshalling.Utf16)]
  private static partial IntPtr CreateEvent(
     IntPtr lpEventAttributes, [MarshalAs(UnmanagedType.Bool)] bool bManualReset,
     [MarshalAs(UnmanagedType.Bool)] bool bInitialState, string? lpName);

  [LibraryImport("kernel32.dll")]
  [return: MarshalAs(UnmanagedType.Bool)]
  private static partial bool SetEvent(IntPtr hEvent);

  [LibraryImport("ole32.dll")]
  private static partial uint CoWaitForMultipleObjects(
      uint dwFlags, uint dwMilliseconds, ulong nHandles,
      IntPtr[] pHandles, out uint dwIndex);

  [LibraryImport("user32.dll")]
  [return: MarshalAs(UnmanagedType.Bool)]
  private static partial bool SetForegroundWindow(IntPtr hWnd);

  private static IntPtr redirectEventHandle = IntPtr.Zero;

  // Do the redirection on another thread, and use a non-blocking
  // wait method to wait for the redirection to complete.
  public static void RedirectActivationTo(AppActivationArguments args,
                                          AppInstance keyInstance)
  {
    redirectEventHandle = CreateEvent(IntPtr.Zero, true, false, null);
    Task.Run(() =>
    {
      keyInstance.RedirectActivationToAsync(args).AsTask().Wait();
      SetEvent(redirectEventHandle);
    });

    uint CWMO_DEFAULT = 0;
    uint INFINITE = 0xFFFFFFFF;
    _ = CoWaitForMultipleObjects(
       CWMO_DEFAULT, INFINITE, 1,
       [redirectEventHandle], out uint handleIndex);

    // Bring the window to the foreground
    Process process = Process.GetProcessById((int)keyInstance.ProcessId);
    SetForegroundWindow(process.MainWindowHandle);
  }
}
