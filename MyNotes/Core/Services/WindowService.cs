using MyNotes.Core.Models;
using MyNotes.Core.Views;

namespace MyNotes.Core.Services;

public class WindowService
{
  public MainWindow? MainWindow { get; private set; } = new();
  public Dictionary<Note, NoteWindow> NoteWindows { get; } = new();
  public Note? CurrentNote { get; private set; }
  // TEST: WeakReference Window
  public List<WeakReference<Window>> Windows = new();
  public List<WeakReference<Page>> Pages = new();
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
      CurrentNote = note;
      NoteWindow newWindow = new();
      // TEST: WeakReference Add Window
      Windows.Add(new(newWindow));
      NoteWindows[note] = newWindow;
      return newWindow;
    }
  }

  public void ActivateNoteWindow(Note note)
  {
    CurrentNote = note;
    NoteWindow newWindow = new();
    newWindow.Activate();
    Windows.Add(new(newWindow));
  }

  public NoteWindow? GetNoteWindow(Note note)
  {
    NoteWindows.TryGetValue(note, out NoteWindow? window);
    return window;
  }

  public bool CloseNoteWindow(Note note)
  {
    return NoteWindows.Remove(note);
  }

  public void ReadActiveWindows()
  {
    GC.Collect();
    Windows.RemoveAll(wr => !wr.TryGetTarget(out _));
    Pages.RemoveAll(wr => !wr.TryGetTarget(out _));
    Debug.WriteLine("Windows: " + string.Join(", ", Windows.Select(
      wr => 
      { 
        wr.TryGetTarget(out Window? target); 
        return target?.GetHashCode().ToString() ?? ""; 
      }
    )));
    Debug.WriteLine("Pages: " + string.Join(", ", Pages.Select(
      wr =>
      {
        wr.TryGetTarget(out Page? target);
        return target?.GetHashCode().ToString() ?? "";
      }
    )));
  }
}
