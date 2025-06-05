using MyNotes.Core.Model;
using MyNotes.Core.ViewModel;

namespace MyNotes.Debugging;

internal static class ReferenceTracker
{
  public static readonly List<WeakReference<Window>> WindowReferences = new();
  public static readonly List<WeakReference<Page>> NavigationPageReferences = new();
  public static readonly List<WeakReference<Note>> NoteReferences = new();
  public static readonly List<WeakReference<Page>> NotePageReferences = new();
  public static readonly List<WeakReference<BoardViewModel>> BoardViewModelReferences = new();
  public static readonly List<WeakReference<NoteViewModel>> NoteViewModelReferences = new();


  public static void ReadActiveWindows()
  {
    Debug.WriteLine("-----------------------------------------------");
    GC.Collect();
    WindowReferences.RemoveAll(wr => !wr.TryGetTarget(out _));
    Debug.WriteLine("Windows: " + string.Join(", ", WindowReferences.Select(
      wr => wr.TryGetTarget(out Window? target) ? target?.GetHashCode().ToString() : "")));

    NavigationPageReferences.RemoveAll(wr => !wr.TryGetTarget(out _));
    Debug.WriteLine("Navigation Pages: " + string.Join(", ", NavigationPageReferences.Select(
      wr => wr.TryGetTarget(out Page? target) ? target.GetHashCode().ToString() : "")));

    BoardViewModelReferences.RemoveAll(wr => !wr.TryGetTarget(out _));
    Debug.WriteLine("Board ViewModel: " + string.Join(", ", BoardViewModelReferences.Select(
      wr => wr.TryGetTarget(out BoardViewModel? target) ? target.GetHashCode().ToString() : "")));

    NoteReferences.RemoveAll(wr => !wr.TryGetTarget(out _));
    Debug.WriteLine("Notes: " + string.Join(", ", NoteReferences.Select(
      wr => wr.TryGetTarget(out Note? target) ? target.GetHashCode().ToString() : "")));

    NotePageReferences.RemoveAll(wr => !wr.TryGetTarget(out _));
    Debug.WriteLine("Note Pages: " + string.Join(", ", NotePageReferences.Select(
      wr => wr.TryGetTarget(out Page? target) ? target.GetHashCode().ToString() : "")));

    NoteViewModelReferences.RemoveAll(wr => !wr.TryGetTarget(out _));
    Debug.WriteLine("Note ViewModel: " + string.Join(", ", NoteViewModelReferences.Select(
      wr => wr.TryGetTarget(out NoteViewModel? target) ? target.GetHashCode().ToString() : "")));

    Debug.WriteLine("");
  }
}
