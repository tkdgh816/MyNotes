namespace MyNotes.Common.Comparers;

public class CompositeComparer<T> : IComparer<T>
{
  private readonly List<IComparer<T>> _comparers = new();

  public void Add(IComparer<T> comparer) => _comparers.Add(comparer);

  public int Compare(T? x, T? y)
  {
    foreach (var comparer in _comparers)
    {
      int result = comparer.Compare(x, y);
      if (result != 0)
        return result;
    }
    return 0;
  }
}
