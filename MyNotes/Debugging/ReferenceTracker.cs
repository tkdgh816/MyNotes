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
    Debug.WriteLine("");
    Debug.WriteLine("---------------------------------------------------------");

    Show("Windows", WindowReferences);
    Show("Controls", ViewReferences);
    Show("Navigation Pages", NavigationPageReferences);
    Show("Board ViewModel", BoardViewModelReferences);
    Show("Notes", NoteReferences);
    Show("Note Pages", NotePageReferences);
    Show("Note ViewModel", NoteViewModelReferences);

    Debug.WriteLine("---------------------------------------------------------");
    Debug.WriteLine("");
  }

  public static void ShowReferencesWithGC()
  {
    GC.Collect();
  }

  static void Show<T>(string text, List<ReferenceTarget<T>> references) where T : class
  {
    references.RemoveAll(rt => !rt.Target.TryGetTarget(out _));
    string itemsList = string.Join(", ", references.Select(rt => rt.Target.TryGetTarget(out T? target) ? $"[{rt.Name}, {target.GetHashCode()}]" : ""));
    Debug.WriteLine($"{text} ({references.Count}): {itemsList[..Math.Min(itemsList.Length, 500)]}");
  }
}

internal class ReferenceTarget<T>(string name, T target) where T: class
{
  public string Name { get; } = name;
  public WeakReference<T> Target { get; } = new(target);
}
