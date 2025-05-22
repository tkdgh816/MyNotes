namespace MyNotes.Common.Collections;

public class SortedObervableCollection<T> : Collection<T>, INotifyCollectionChanged
{
  private readonly List<T> _items;

  public SortedObervableCollection() => _items = (List<T>)Items;
  public SortedObervableCollection(IEnumerable<T> items) : this()
  {
    foreach (T item in items)
      Add(item);
  }

  public event NotifyCollectionChangedEventHandler? CollectionChanged;
  protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e) => CollectionChanged?.Invoke(this, e);

  public new T this[int index]
  {
    get => _items[index];
    set
    {
      if (_items[index]?.Equals(value) ?? true)
        return;
      T oldItem = _items[index];
      _items[index] = value;
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldItem));
    }
  }

  public new void Add(T item)
  {
    int index = _items.BinarySearch(item);
    if (index < 0)
    {
      int insertedIndex = ~index;
      _items.Insert(insertedIndex, item);
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, insertedIndex));
    }
  }

  public new void Clear()
  {
    _items.Clear();
    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
  }

  public new bool Remove(T item)
  {
    int itemIndex = IndexOf(item);
    if (itemIndex < 0 || itemIndex >= Count)
      return false;
    _items.RemoveAt(itemIndex);
    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, itemIndex));
    return true;
  }
}