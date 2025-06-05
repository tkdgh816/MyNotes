using MyNotes.Common.Messaging;
using MyNotes.Core.Model;
using MyNotes.Core.View;
using MyNotes.Debugging;

namespace MyNotes.Core.Service;

internal class WindowService
{
  public MainWindow? MainWindow { get; private set; } = new();
  public Dictionary<Note, NoteWindow> NoteWindows { get; } = new();

  public WindowService()
  {

  }

  public MainWindow GetMainWindow() => MainWindow ??= new MainWindow();
  public void DestroyMainWindow() => MainWindow = null;

  public NoteWindow CreateNoteWindow(Note note)
  {
    NoteWindow? window = GetNoteWindow(note);
    if (window is not null)
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

  private double _dpi = 1.25;
  private void InitializeNoteWindow(Note note, NoteWindow noteWindow)
  {
    var presenter = (OverlappedPresenter)noteWindow.AppWindow.Presenter;
    presenter.PreferredMinimumWidth = (int)(350 * _dpi);
    presenter.PreferredMinimumHeight = (int)(350 * _dpi);
    presenter.SetBorderAndTitleBar(true, false);

    var windowActivatedEventHandler = CreateNoteWindowActivatedEventHandler(note);
    var appWindowChangedEventHandler = CreateNoteAppWindowChangedEventHandler(note);

    noteWindow.Activated += windowActivatedEventHandler;
    noteWindow.AppWindow.Changed += appWindowChangedEventHandler;

    noteWindow.Closed += (sender, args) =>
    {
      var window = (NoteWindow)sender;
      window.Activated -= windowActivatedEventHandler;
      window.AppWindow.Changed -= appWindowChangedEventHandler;
    };
  }

  private TypedEventHandler<object, WindowActivatedEventArgs> CreateNoteWindowActivatedEventHandler(Note note)
  {
    return (sender, args) =>
    {
      switch (args.WindowActivationState)
      {
        case WindowActivationState.Deactivated:
          WeakReferenceMessenger.Default.Send(new Message(note, false), Tokens.ToggleNoteWindowActivation);
          break;
        default:
          WeakReferenceMessenger.Default.Send(new Message(note, true), Tokens.ToggleNoteWindowActivation);
          break;
      }
    };
  }
  private TypedEventHandler<AppWindow, AppWindowChangedEventArgs> CreateNoteAppWindowChangedEventHandler(Note note)
  {
    return (appWindow, args) =>
    {
      if (args.DidSizeChange)
        WeakReferenceMessenger.Default.Send(new Message(note, appWindow.ClientSize), Tokens.ResizeNoteWindow);
      if (args.DidPositionChange)
        WeakReferenceMessenger.Default.Send(new Message(note, appWindow.Position), Tokens.MoveNoteWindow);
    };
  }

  public void ActivateNoteWindow(Note note)
  {
    NoteWindow newWindow = new(note);
    newWindow.Activate();
    ReferenceTracker.WindowReferences.Add(new(newWindow));
  }

  public NoteWindow? GetNoteWindow(Note note)
  {
    NoteWindows.TryGetValue(note, out NoteWindow? window);
    return window;
  }

  public bool IsNoteWindowActive(Note note) => NoteWindows.TryGetValue(note, out var _);

  public bool CloseNoteWindow(Note note)
  {
    if (NoteWindows.TryGetValue(note, out NoteWindow? window))
    {
      window.Close();
      return NoteWindows.Remove(note);
    }
    return false;
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
}
