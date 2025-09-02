using System.Runtime.InteropServices;

using WinRT.Interop;

namespace MyNotes.Common.Interop;

internal static partial class NativeMethods
{
  [LibraryImport("user32.dll")]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static partial bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

  [LibraryImport("user32.dll")]
  public static partial IntPtr MonitorFromRect(ref RECT lprc, uint dwFlags);

  [LibraryImport("user32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static partial bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

  [LibraryImport("user32.dll")]
  public static partial uint GetDpiForWindow(IntPtr hWnd);

  [StructLayout(LayoutKind.Sequential)]
  public struct RECT
  {
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct MONITORINFO
  {
    public int cbSize;
    public RECT rcMonitor;
    public RECT rcWork;
    public uint dwFlags;
  }

  enum MonitorDefaultFlags : uint
  {
    DefaultToNull    = 0x00000000,  // MONITOR_DEFAULTTONULL
    DefaultToPrimary = 0x00000001,  // MONITOR_DEFAULTTOPRIMARY
    DefaultToNearest = 0x00000002   // MONITOR_DEFAULTTONEAREST
  }

  public static IntPtr GetWindowHandle(Window window) => WindowNative.GetWindowHandle(window);

  public static double GetWindowScaleFactor(IntPtr hWnd) => GetDpiForWindow(hWnd) / 96.0;

  public static MONITORINFO? GetMonitorInfoForWindow(IntPtr hWnd)
  {
    if (!GetWindowRect(hWnd, out var rect))
      return null;

    IntPtr hMonitor = MonitorFromRect(ref rect, (uint)MonitorDefaultFlags.DefaultToNearest);

    MONITORINFO monitorInfo = new() { cbSize = Marshal.SizeOf<MONITORINFO>() };
    return !GetMonitorInfo(hMonitor, ref monitorInfo) ? null : monitorInfo;
  }
}
