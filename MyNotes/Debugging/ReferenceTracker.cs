using MyNotes.Core.Model;
using MyNotes.Core.ViewModel;

namespace MyNotes.Debugging;

internal static class ReferenceTracker
{
  public static readonly List<ReferenceTarget<Window>> WindowReferences = new();
  public static readonly List<ReferenceTarget<UserControl>> ViewReferences = new();
  public static readonly List<ReferenceTarget<Page>> NavigationPageReferences = new();
  public static readonly List<ReferenceTarget<Note>> NoteReferences = new();
  public static readonly List<ReferenceTarget<Page>> NotePageReferences = new();
  public static readonly List<ReferenceTarget<BoardViewModel>> BoardViewModelReferences = new();
  public static readonly List<ReferenceTarget<NoteViewModel>> NoteViewModelReferences = new();

  public static void ShowReferences()
  {
    Debug.WriteLine("-----------------------------------------------");
    GC.Collect();
    WindowReferences.RemoveAll(rt => !rt.Target.TryGetTarget(out _));
    Debug.WriteLine("Windows: " + string.Join(", ", WindowReferences.Select(
      rt => rt.Target.TryGetTarget(out Window? target) ? $"[{rt.Name}, {target.GetHashCode()}]" : "")));

    ViewReferences.RemoveAll(rt => !rt.Target.TryGetTarget(out _));
    Debug.WriteLine("Controls: " + string.Join(", ", ViewReferences.Select(
      rt => rt.Target.TryGetTarget(out UserControl? target) ? $"[{rt.Name}, {target.GetHashCode()}]" : "")));

    NavigationPageReferences.RemoveAll(rt => !rt.Target.TryGetTarget(out _));
    Debug.WriteLine("Navigation Pages: " + string.Join(", ", NavigationPageReferences.Select(
      rt => rt.Target.TryGetTarget(out Page? target) ? $"[{rt.Name}, {target.GetHashCode()}]" : "")));

    BoardViewModelReferences.RemoveAll(rt => !rt.Target.TryGetTarget(out _));
    Debug.WriteLine("Board ViewModel: " + string.Join(", ", BoardViewModelReferences.Select(
      rt => rt.Target.TryGetTarget(out BoardViewModel? target) ? $"[{rt.Name}, {target.GetHashCode()}]" : "")));

    NoteReferences.RemoveAll(rt => !rt.Target.TryGetTarget(out _));
    Debug.WriteLine("Notes: " + string.Join(", ", NoteReferences.Select(
      rt => rt.Target.TryGetTarget(out Note? target) ? $"[{rt.Name}, {target.GetHashCode()}]" : "")));

    NotePageReferences.RemoveAll(rt => !rt.Target.TryGetTarget(out _));
    Debug.WriteLine("Note Pages: " + string.Join(", ", NotePageReferences.Select(
      rt => rt.Target.TryGetTarget(out Page? target) ? $"[{rt.Name}, {target.GetHashCode()}]" : "")));

    NoteViewModelReferences.RemoveAll(rt => !rt.Target.TryGetTarget(out _));
    Debug.WriteLine("Note ViewModel: " + string.Join(", ", NoteViewModelReferences.Select(
      rt => rt.Target.TryGetTarget(out NoteViewModel? target) ? $"[{rt.Name}, {target.GetHashCode()}]" : "")));

    Debug.WriteLine("");
  }
}

internal class ReferenceTarget<T>(string name, T target) where T: class
{
  public string Name { get; } = name;
  public WeakReference<T> Target { get; } = new(target);
}
