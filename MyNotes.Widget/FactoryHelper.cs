using System;
using System.Runtime.InteropServices;

using Microsoft.Windows.Widgets.Providers;

using WinRT;

namespace MyNotes.Widget;

public static class Guids
{
  public const string IClassFactory = "00000001-0000-0000-C000-000000000046";
  public const string IUnknown = "00000000-0000-0000-C000-000000000046";
}

[ComImport, ComVisible(false), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid(Guids.IClassFactory)]
public interface IClassFactory
{
  [PreserveSig]
  int CreateInstance(IntPtr pUnkOuter, ref Guid riid, out IntPtr ppvObject);
  [PreserveSig]
  int LockServer(bool fLock);
}

[ComVisible(true)]
public class WidgetProviderFactory<T> : IClassFactory where T : IWidgetProvider, new()
{
  public T? Instance { get; private set; }
  public int CreateInstance(IntPtr pUnkOuter, ref Guid riid, out IntPtr ppvObject)
  {
    Instance = default;
    ppvObject = IntPtr.Zero;

    if (pUnkOuter != IntPtr.Zero)
    {
      Marshal.ThrowExceptionForHR(CLASS_E_NOAGGREGATION);
    }

    if (riid == typeof(T).GUID || riid == Guid.Parse(Guids.IUnknown))
    {
      // Create the instance of the .NET object
      Instance = new T();
      ppvObject = MarshalInspectable<IWidgetProvider>.FromManaged(Instance);
    }
    else
    {
      // The object that ppvObject points to does not support the
      // interface identified by riid.
      Marshal.ThrowExceptionForHR(E_NOINTERFACE);
    }

    return 0;
  }

  int IClassFactory.LockServer(bool fLock)
  {
    return 0;
  }

  private const int CLASS_E_NOAGGREGATION = -2147221232;
  private const int E_NOINTERFACE = -2147467262;
}
