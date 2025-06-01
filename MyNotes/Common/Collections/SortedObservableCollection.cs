using MyNotes.Common.Comparers;

namespace MyNotes.Common.Collections;

public class SortedObervableCollection<T> : Collection<T>, INotifyCollectionChanged
{
  public SortedObervableCollection() : base(new List<T>())
  {
    SortDescriptions = new();
    SortDescriptions.CollectionChanged += OnSortDescripTionChanged;
    if (this is IComparable<T>)
      _comparers.Add(Comparer<T>.Default);
  }

  public SortedObervableCollection(IEnumerable<SortDescription<T>> sortDescriptions) : this()
  {
    SortDescriptions = new(sortDescriptions);
    SortDescriptions.CollectionChanged += OnSortDescripTionChanged;
    InitializeComparer();
  }

  public SortedObervableCollection(IEnumerable<T> items, IEnumerable<SortDescription<T>>? sortDescriptions = null) : this()
  {
    if (sortDescriptions is not null)
    {
      SortDescriptions = new(sortDescriptions);
      SortDescriptions.CollectionChanged += OnSortDescripTionChanged;
      InitializeComparer();
    }
    else
    {
      if (this is IComparable<T>)
        _comparers.Add(Comparer<T>.Default);
    }
    AddRange(items);
  }

  public ObservableCollection<SortDescription<T>> SortDescriptions { get; }
  private CompositeComparer<T> _comparers = new();
  public HashSet<string> SortKeys { get; } = new();

  private void InitializeComparer()
  {
    _comparers = new();
    SortKeys.Clear();
    foreach (SortDescription<T> sortDescription in SortDescriptions!)
    {
      if (sortDescription.KeyPropertyName is not null)
        SortKeys.Add(sortDescription.KeyPropertyName);
      _comparers.Add(sortDescription.Comparer);
    }
  }
  private void OnSortDescripTionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    => InitializeComparer();

  public event NotifyCollectionChangedEventHandler? CollectionChanged;
  protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e) => CollectionChanged?.Invoke(this, e);

  protected override void InsertItem(int index, T item)
  {
    int insertedIndex = FindInsertIndex(BinarySearch(item));

    if (item is INotifyPropertyChanged observableItem)
      observableItem.PropertyChanged += OnItemPropertyChanged;

    base.InsertItem(insertedIndex, item);
    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, insertedIndex));
  }

  private int FindInsertIndex(int index)
    => (index >= 0) ? FindRange(index).End.Value + 1 : ~index;


  protected override void ClearItems()
  {
    foreach (T item in this)
      if (item is INotifyPropertyChanged observableItem)
        observableItem.PropertyChanged -= OnItemPropertyChanged;

    base.ClearItems();
    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
  }

  protected override void RemoveItem(int index)
  {
    T item = this[index];

    if (item is INotifyPropertyChanged observableItem)
      observableItem.PropertyChanged -= OnItemPropertyChanged;

    base.RemoveItem(index);
    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
  }

  protected override void SetItem(int index, T item)
  {
    if (this[index]?.Equals(item) ?? true)
      return;
    T oldItem = this[index];
    base.SetItem(index, item);
    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem));
  }

  public void Refresh()
  {
    var temp = this.ToArray();
    Clear();
    AddRange(temp);
  }

  private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (sender is T item && e.PropertyName is not null && SortKeys.Contains(e.PropertyName))
    {
      int oldIndex = IndexOf(item);
      Debug.WriteLine($"{item}: {e.PropertyName}");
      if (oldIndex < 0 || !CheckItemMove(oldIndex))
        return;
      base.RemoveItem(oldIndex);
      int newIndex = FindInsertIndex(BinarySearch(item));
      base.InsertItem(newIndex, item);
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
    }
  }

  private bool CheckItemMove(int index)
  {
    if (Count <= 1)
      return false;
    if (index == 0)
      return _comparers.Compare(this[index], this[index + 1]) > 0;
    else if (index == Count - 1)
      return _comparers.Compare(this[index], this[index - 1]) < 0;
    else
      return _comparers.Compare(this[index], this[index + 1]) > 0 || _comparers.Compare(this[index], this[index - 1]) < 0;
  }

  public void AddRange(IEnumerable<T> collection)
  {
    int size = Count;
    int count = collection.Count();

    ((List<T>)Items).EnsureCapacity(size + count);
    foreach (T item in collection)
      Add(item);
  }

  private Range FindRange(int index)
  {
    int startIndex = index, endIndex = index;
    while (startIndex > 0 && _comparers.Compare(this[startIndex - 1], this[index]) == 0)
      startIndex--;
    while (endIndex < Count - 1 && _comparers.Compare(this[endIndex + 1], this[index]) == 0)
      endIndex++;
    return new(startIndex, endIndex);
  }

  public int BinarySearch(T item)
  {
    return ((List<T>)Items).BinarySearch(item, _comparers);
  }
}