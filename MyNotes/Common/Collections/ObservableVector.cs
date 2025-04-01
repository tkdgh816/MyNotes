namespace MyNotes.Common.Collections;

public class ObservableVector<T> : IObservableVector<T>
{
  private readonly List<T> _list = new();

  public event VectorChangedEventHandler<T>? VectorChanged;

  private void OnVectorChanged(CollectionChange collectionChange, object? item = null, int index = -1)
    => VectorChanged?.Invoke(this, new VectorChangedEventArgs(collectionChange, item, index));

  public T this[int index]
  {
    get => _list[index];
    set
    {
      _list[index] = value;
      OnVectorChanged(CollectionChange.ItemChanged, value, index);
    }
  }

  public int Count => _list.Count;

  public bool IsReadOnly => false;

  public void Add(T item)
  {
    _list.Add(item);
    OnVectorChanged(CollectionChange.ItemInserted, item, _list.Count - 1);
  }

  public void Clear()
  {
    _list.Clear();
    OnVectorChanged(CollectionChange.Reset);
  }

  public bool Contains(T item) => _list.Contains(item);

  public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

  public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

  IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

  public int IndexOf(T item) => _list.IndexOf(item);

  public void Insert(int index, T item)
  {
    _list.Insert(index, item);
    OnVectorChanged(CollectionChange.ItemInserted, item, index);
  }

  public bool Remove(T item)
  {
    int index = _list.IndexOf(item);
    if (index >= 0)
    {
      RemoveAt(index, item);
      return true;
    }
    return false;
  }

  public void RemoveAt(int index) => RemoveAt(index, _list[index]);

  private void RemoveAt(int index, T item)
  {
    _list.RemoveAt(index);
    OnVectorChanged(CollectionChange.ItemRemoved, item, index);
  }
}

public class VectorChangedEventArgs(CollectionChange collectionChange, object? item = null, int index = -1) : IVectorChangedEventArgs
{
  public CollectionChange CollectionChange { get; } = collectionChange;
  public object? Item { get; } = item;
  public uint Index { get; } = index >= 0 ? (uint)index : uint.MaxValue;
}