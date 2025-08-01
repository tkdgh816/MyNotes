namespace MyNotes.Common.Collections;
internal class SortedCollection<T> : IList<T>
{
  private readonly List<T> _inner = new();
  private readonly IComparer<T> _comparer;

  public SortedCollection() : this(Comparer<T>.Default) { }

  public SortedCollection(IComparer<T> comparer) 
    => _comparer = comparer ?? Comparer<T>.Default;

  public void Add(T item)
  {
    int index = _inner.BinarySearch(item, _comparer);
    if (index < 0)
      index = ~index;
    _inner.Insert(index, item);
  }

  public bool Remove(T item) => _inner.Remove(item);
  public void Clear() => _inner.Clear();
  public bool Contains(T item) => _inner.Contains(item);
  public int Count => _inner.Count;
  public bool IsReadOnly => false;

  public T this[int index]
  {
    get => _inner[index];
    set => throw new NotSupportedException("SortedCollection cannot be set to values by index.");
  }

  public int IndexOf(T item) => _inner.IndexOf(item);
  public void Insert(int index, T item) => Add(item);
  public void RemoveAt(int index) => _inner.RemoveAt(index);
  public void CopyTo(T[] array, int arrayIndex) => _inner.CopyTo(array, arrayIndex);

  public IEnumerator<T> GetEnumerator() => _inner.GetEnumerator();
  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
