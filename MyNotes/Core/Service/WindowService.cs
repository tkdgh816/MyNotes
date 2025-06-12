using MyNotes.Common.Messaging;
using MyNotes.Core.Model;
using MyNotes.Core.View;
using MyNotes.Debugging;

using static MyNotes.Common.Interop.NativeMethods;

namespace MyNotes.Core.Service;

internal class WindowService
{
  public WindowService()
  {
  }

  #region MainWindow
  public MainWindow? MainWindow { get; private set; } = null;

  public MainWindow GetMainWindow()
  {
    if (MainWindow != null)
      return MainWindow;

    MainWindow window = new();
    var hWnd = GetWindowHandle(window);
    var presenter = (OverlappedPresenter)window.AppWindow.Presenter;
    double dpiScale = GetWindowScaleFactor(hWnd);

    window.ExtendsContentIntoTitleBar = true;
    presenter.PreferredMinimumWidth = (int)(400 * dpiScale);
    presenter.PreferredMinimumHeight = (int)(600 * dpiScale);
    window.Closed += OnMainWindowClosed;

    ReferenceTracker.WindowReferences.Add(new(window));

    MainWindow = window;
    return MainWindow;
  }

  private void OnMainWindowClosed(object sender, WindowEventArgs args)
  {
    MainWindow = null;
  }
  #endregion

  #region NoteWindows
  public Dictionary<Note, NoteWindow> NoteWindows { get; } = new();

  public bool IsNoteWindowActive(Note note) => NoteWindows.TryGetValue(note, out var _);

  public NoteWindow GetNoteWindow(Note note)
  {
    if (NoteWindows.TryGetValue(note, out NoteWindow? window))
      return window;
    else
    {
      NoteWindow newWindow = new(note);
      // TEST: WeakReference Add Window
      ReferenceTracker.WindowReferences.Add(new(newWindow));
      NoteWindows[note] = newWindow;
      InitializeNoteWindow(note, newWindow);
      return newWindow;
    }
  }

  public bool CloseNoteWindow(Note note)
  {
    if (NoteWindows.TryGetValue(note, out NoteWindow? window))
    {
      window.Close();
      return true;
    }
    return false;
  }

  private void InitializeNoteWindow(Note note, NoteWindow noteWindow)
  {
    var hWnd = GetWindowHandle(noteWindow);
    var dpiScale = GetWindowScaleFactor(hWnd);
    var appWindow = noteWindow.AppWindow;
    var presenter = (OverlappedPresenter)appWindow.Presenter;
    var displayArea = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Nearest);

    // Presenter: Minimum size, Set TitleBar
    presenter.PreferredMinimumWidth = (int)(350 * dpiScale);
    presenter.PreferredMinimumHeight = (int)(350 * dpiScale);
    presenter.SetBorderAndTitleBar(true, false);

    // Move and Resize
    var size = note.Size;
    var position = note.Position;

    if (position.X == -1 && position.Y == -1)
      appWindow.Resize(new SizeInt32()
      {
        Width = (int)(size.Width * dpiScale),
        Height = (int)(size.Height * dpiScale)
      });
    else
    {
      var workArea = displayArea.WorkArea;
      appWindow.MoveAndResize(new RectInt32()
      {
        X = Math.Clamp(position.X, workArea.X, workArea.Width),
        Y = Math.Clamp(position.Y, workArea.Y, workArea.Height),
        Width = (int)(size.Width * dpiScale),
        Height = (int)(size.Height * dpiScale)
      });
    }

    // Register Events
    var windowActivatedEventHandler = CreateNoteWindowActivatedEventHandler(note);
    var appWindowChangedEventHandler = CreateNoteAppWindowChangedEventHandler(note);

    noteWindow.Activated += windowActivatedEventHandler;
    appWindow.Changed += appWindowChangedEventHandler;

    noteWindow.Closed += (sender, args) =>
    {
      var window = (NoteWindow)sender;
      NoteWindows.Remove(note);
      window.Activated -= windowActivatedEventHandler;
      appWindow.Changed -= appWindowChangedEventHandler;
    };
  }

  private TypedEventHandler<object, WindowActivatedEventArgs> CreateNoteWindowActivatedEventHandler(Note note)
    => (sender, args) =>
    {
      switch (args.WindowActivationState)
      {
        case WindowActivationState.Deactivated:
          WeakReferenceMessenger.Default.Send(new Message<bool>(false, note), Tokens.ToggleNoteWindowActivation);
          break;
        default:
          WeakReferenceMessenger.Default.Send(new Message<bool>(true, note), Tokens.ToggleNoteWindowActivation);
          break;
      }
    };
  private TypedEventHandler<AppWindow, AppWindowChangedEventArgs> CreateNoteAppWindowChangedEventHandler(Note note)
    => (appWindow, args) =>
    {
      if (args.DidSizeChange)
        WeakReferenceMessenger.Default.Send(new Message<SizeInt32>(appWindow.ClientSize, note), Tokens.ResizeNoteWindow);
      if (args.DidPositionChange)
        WeakReferenceMessenger.Default.Send(new Message<PointInt32>(appWindow.Position, note), Tokens.MoveNoteWindow);
    };

  public void ActivateNoteWindow(Note note)
  {
    NoteWindow newWindow = new(note);
    newWindow.Activate();
    ReferenceTracker.WindowReferences.Add(new(newWindow));
  }

  public void SetNoteWindowTitleBar(Note note, UIElement titleBar)
  {
    if (NoteWindows.TryGetValue(note, out NoteWindow? window))
      window.SetTitleBar(titleBar);
  }

  public void MinimizeNoteWindow(Note note)
  {
    if (NoteWindows.TryGetValue(note, out NoteWindow? window))
      ((OverlappedPresenter)window.AppWindow.Presenter).Minimize();
  }

  public void ToggleNoteWindowPin(Note note, bool isAlwaysOnTop)
  {
    if (NoteWindows.TryGetValue(note, out NoteWindow? window))
      ((OverlappedPresenter)window.AppWindow.Presenter).IsAlwaysOnTop = isAlwaysOnTop;
  }

  public void SetNoteWindowRegionRects(Note note, RectInt32[]? rects)
  {
    if (NoteWindows.TryGetValue(note, out NoteWindow? window))
    {
      var nonClientInputSrc = InputNonClientPointerSource.GetForWindowId(window.AppWindow.Id);
      nonClientInputSrc.SetRegionRects(NonClientRegionKind.Passthrough, rects);
    }
  }

  public void SetNoteWindowBackdrop(Note note, BackdropKind backdropKind)
  {
    if (NoteWindows.TryGetValue(note, out NoteWindow? window))
    {
      window.SystemBackdrop = backdropKind switch
      {
        BackdropKind.Acrylic => new DesktopAcrylicBackdrop(),
        BackdropKind.Mica => new MicaBackdrop(),
        BackdropKind.None or _ => null,
      };
    }
  }
  #endregion
}
