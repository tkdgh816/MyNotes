using System;
using System.Runtime.InteropServices;

using Microsoft.UI.Xaml;

using WinRT.Interop;

namespace MyNotes.Common.Interop;

public static partial class NativeMethods
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
    DefaultToNull = 0x00000000,  // MONITOR_DEFAULTTONULL
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

  // For single AppInstance
  [LibraryImport("kernel32.dll", EntryPoint = "CreateEventW", StringMarshalling = StringMarshalling.Utf16)]
  public static partial IntPtr CreateEvent(IntPtr lpEventAttributes, [MarshalAs(UnmanagedType.Bool)] bool bManualReset, [MarshalAs(UnmanagedType.Bool)] bool bInitialState, string? lpName);

  [LibraryImport("kernel32.dll")]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static partial bool SetEvent(IntPtr hEvent);

  [LibraryImport("ole32.dll")]
  public static partial uint CoWaitForMultipleObjects(uint dwFlags, uint dwMilliseconds, ulong nHandles, IntPtr[] pHandles, out uint dwIndex);

  [LibraryImport("user32.dll")]
  [return: MarshalAs(UnmanagedType.Bool)]
  public static partial bool SetForegroundWindow(IntPtr hWnd);
}
