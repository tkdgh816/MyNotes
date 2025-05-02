using MyNotes.Core.Models;
using MyNotes.Core.Views;

namespace MyNotes.Core.Services;

public class WindowService
{
  public MainWindow? MainWindow { get; private set; } = new();
  public Dictionary<Note, NoteWindow> NoteWindows { get; } = new();
  public Note? CurrentNote { get; private set; }

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
      NoteWindows[note] = newWindow;
      return newWindow;
    }
  }

  public NoteWindow? GetNoteWindow(Note note)
  {
    NoteWindows.TryGetValue(note, out NoteWindow? window);
    return window;
  }

  public bool CloseNoteWindow(Note note)
  {
    if (NoteWindows.TryGetValue(note, out NoteWindow? window))
    {
      window.Close();
      NoteWindows.Remove(note);
      return true;
    }
    return false;
  }
}
