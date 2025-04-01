using MyNotes.Common.Comparers;

namespace MyNotes.Common.Collections;

#region CollectionSource: ICollectionViewFactory
public class CollectionSource<T> : ObservableCollection<T>, ICollectionViewFactory where T : notnull
{
  public bool IsReadOnly { get; set; } = false;

  // Filter
  private Predicate<T>? _filter;
  public Predicate<T>? Filter
  {
    get => _filter;
    set
    {
      FilterChanging?.Invoke(this, EventArgs.Empty);
      _filter = value;
      FilterChanged?.Invoke(this, EventArgs.Empty);
    }
  }
  public bool IsSourceFiltered { get => Filter is not null; }
  public event EventHandler? FilterChanging;
  public event EventHandler? FilterChanged;

  // Sort
  private ObservableCollection<SortDescription<T>>? _sortDescriptions;
  public ObservableCollection<SortDescription<T>>? SortDescriptions
  {
    get => _sortDescriptions;
    set
    {
      _sortDescriptions = value;
      SortDescriptionsChanged?.Invoke(this, EventArgs.Empty);
    }
  }
  public bool IsSourceSorted { get => SortDescriptions?.Any() ?? false; }
  public event EventHandler? SortDescriptionsChanged;

  // Group
  public GroupDescription<T>? GroupDescription { get; init; }
  public bool IsSourceGrouped { get => GroupDescription is not null; }

  // View
  CollectionView _collectionView;
  public void RefreshView() => _collectionView.Refresh();

  public CollectionSource(Predicate<T>? filter = null, IEnumerable<SortDescription<T>>? sortDescriptions = null, GroupDescription<T>? groupDescription = null)
  {
    if (filter is not null)
      Filter = filter;
    SortDescriptions = (sortDescriptions is null) ? null : new(sortDescriptions);
    GroupDescription = groupDescription;
    _collectionView = new(this);
  }

  public CollectionSource(IEnumerable<T> source, Predicate<T>? filter = null, IEnumerable<SortDescription<T>>? sortDescriptions = null, GroupDescription<T>? groupDescription = null) : this(filter, sortDescriptions, groupDescription)
    => AddRange(source);

  public ICollectionView CreateView() => _collectionView;

  public void AddRange(params T[] items)
  {
    foreach (T item in items)
      Add(item);
  }
  public void AddRange(IEnumerable<T> items)
  {
    foreach (T item in items)
      Add(item);
  }

  public void InsertRange(int index, params T[] items)
  {
    foreach (T item in items)
      Insert(index++, item);
  }
  public void InsertRange(int index, IEnumerable<T> items)
  {
    foreach (T item in items)
      Insert(index++, item);
  }

  // _collectionView : ICollectionView
  #region CollectionView : ICollectionView
  private class CollectionView : ICollectionView
  {
    public CollectionSource<T> Source;

    public CollectionView(CollectionSource<T> source)
    {
      Source = source;
      Source.CollectionChanged += Source_CollectionChanged;
      Source.FilterChanging += FilterChanging;
      Source.FilterChanged += FilterChanged;
      Source.SortDescriptionsChanged += SortDescriptionsChanged;
    }

    public List<T> View { get; } = new();

    private void Source_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          HandleItemAdded(e.NewStartingIndex, e.NewItems);
          break;
        case NotifyCollectionChangedAction.Remove:
          HandleItemRemoved(e.OldStartingIndex, e.OldItems);
          break;
        case NotifyCollectionChangedAction.Replace:
          HandleItemReplaced(e.OldItems, e.NewItems);
          break;
        case NotifyCollectionChangedAction.Move:
          HandleItemMoved(e.OldStartingIndex, e.OldItems, e.NewStartingIndex);
          break;
        case NotifyCollectionChangedAction.Reset:
          HandleSourceReset();
          break;
      }
    }

    #region HandleItemChanged
    // Added
    private bool HandleItemAdded(int newStartingIndex, T newItem)
    {
      int viewIndex = newStartingIndex;
      if (Source.IsSourceFiltered)
      {
        if (InvokeFilter(newItem))
          viewIndex = GetFilteredViewIndex(viewIndex);
        else
          return false;
      }

      if (Source.IsSourceSorted)
        viewIndex = GetSortedViewIndex(newItem);
      View.Insert(viewIndex, newItem);

      if (Source.IsSourceGrouped)
        viewIndex = InsertIntoGroup(viewIndex, newItem);
      OnVectorChanged(CollectionChange.ItemInserted, newItem, viewIndex);
      return true;
    }

    private void HandleItemAdded(int newStartingIndex, IList? newItems)
    {
      if (newItems is null)
        return;

      int itemCount = 0;
      foreach (T newItem in newItems)
      {
        if (HandleItemAdded(newStartingIndex + itemCount, newItem))
          itemCount++;
      }
    }

    // Removed
    private bool HandleItemRemoved(int oldStartingIndex, T oldItem)
    {
      int viewIndex = oldStartingIndex;
      if (Source.IsSourceFiltered)
      {
        if (InvokeFilter(oldItem))
          viewIndex = View.IndexOf(oldItem);
        else
          return false;
      }

      if (viewIndex >= 0 && viewIndex < View.Count)
      {
        View.RemoveAt(viewIndex);

        if (Source.IsSourceGrouped)
          viewIndex = RemoveFromGroup(oldItem);

        OnVectorChanged(CollectionChange.ItemRemoved, oldItem, viewIndex);
        return true;
      }

      return false;
    }

    private void HandleItemRemoved(int oldStartingIndex, IList? oldItems)
    {
      if (oldItems is null)
        return;

      foreach (T oldItem in oldItems)
        HandleItemRemoved(oldStartingIndex, oldItem);
    }

    // Replaced
    private void HandleItemReplaced(IList? oldItems, IList? newItems)
    {
      if (oldItems is null || newItems is null || oldItems.Count != newItems.Count)
        return;

      for (int i = 0; i < oldItems.Count; i++)
      {
        if (oldItems[i] is not T oldItem || newItems[i] is not T newItem)
          continue;

        int viewIndex = View.IndexOf(oldItem);
        if (Source.IsSourceFiltered && !InvokeFilter(newItem))
        {
          HandleItemRemoved(viewIndex, View[viewIndex]);
          continue;
        }

        View[viewIndex] = newItem;
        if (Source.IsSourceGrouped)
        {
          if (ReplaceInGroup(oldItem, newItem, out int oldViewIndex, out int newViewIndex))
            OnVectorChanged(CollectionChange.ItemChanged, newItem, newViewIndex);
          else
          {
            OnVectorChanged(CollectionChange.ItemRemoved, oldItem, oldViewIndex);
            OnVectorChanged(CollectionChange.ItemInserted, newItem, newViewIndex);
          }
        }
        else
          OnVectorChanged(CollectionChange.ItemChanged, newItem, viewIndex);
      }
    }

    // Moved
    private void HandleItemMoved(int oldStartingIndex, IList? oldItems, int newStartingIndex)
    {
      if (oldItems is null)
        return;
      HandleItemRemoved(oldStartingIndex, oldItems);
      HandleItemAdded(newStartingIndex, oldItems);
    }

    // Reset
    private void HandleSourceReset()
    {
      //HandleItemRemoved(0, Source);
      _collectionGroups.Clear();
      View.Clear();
      HandleItemAdded(0, Source);
      OnVectorChanged(CollectionChange.Reset);
    }

    public void Refresh() => HandleSourceReset();
    #endregion

    #region Filter
    private bool _isSourceReset = false;

    private void FilterChanging(object? sender, EventArgs e)
    {
      if (Source.Count > 0)
      {
        HandleItemRemoved(0, Source);
        _isSourceReset = true;
      }
    }

    private void FilterChanged(object? sender, EventArgs e)
    {
      if (_isSourceReset)
      {
        HandleItemAdded(0, Source);
        VectorChanged?.Invoke(this, new VectorChangedEventArgs(CollectionChange.Reset));
        _isSourceReset = false;
      }
    }

    private int GetFilteredViewIndex(int sourceIndex)
    {
      int viewIndex = 0;
      if (sourceIndex == Source.Count - 1)
        viewIndex = View.Count;
      else
        for (int index = 0; index < Source.Count; index++)
        {
          if (index == sourceIndex)
            break;
          if (View[viewIndex].Equals(Source[index]))
            viewIndex++;
        }
      return viewIndex;
    }

    public bool InvokeFilter(T item)
    {
      if (Source.Filter is null)
        return true;

      foreach (var predicate in Source.Filter.GetInvocationList().Cast<Predicate<T>>())
        if (!predicate.Invoke(item))
          return false;

      return true;
    }
    #endregion

    #region Sort
    private void SortDescriptionsChanged(object? sender, EventArgs e)
    {
      if (Source.SortDescriptions is not null)
      {
        if (Source.Count > 0)
          HandleSourceReset();
        Source.SortDescriptions.CollectionChanged += SortDescriptions_CollectionChanged;
      }
    }

    private void SortDescriptions_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
      => HandleSourceReset();

    private int GetSortedViewIndex(T item)
    {
      if (Source.SortDescriptions is null)
        return -1;
      int searchStartIndex = 0;
      int searchLength = View.Count;
      CompositeComparer<T> comparer = new();
      foreach (SortDescription<T> sortDescription in Source.SortDescriptions)
        comparer.Add(sortDescription.Comparer);
      int targetIndex = View.BinarySearch(searchStartIndex, searchLength, item, comparer);
      if (targetIndex < 0)
        targetIndex = ~targetIndex;
      return targetIndex;
    }
    #endregion

    #region Group
    private ObservableVector<object> _collectionGroups = new();
    public IObservableVector<object>? CollectionGroups => Source.IsSourceGrouped ? _collectionGroups : null;

    private object GetGroupKey(T item)
    {
      var groupDescription = Source.GroupDescription!;
      if (groupDescription.KeyProperty is not null)
        return groupDescription.KeyPropertyInfo?.GetValue(item) ?? throw new ArgumentException("GroupKey Not Found");
      else if (groupDescription.KeySelector is not null)
        return groupDescription.KeySelector.Invoke(item);
      else
        throw new ArgumentException("GroupKey Not Found");
    }

    private CollectionViewGroup GetGroup(T item, bool createIfNotExists = false)
    {
      var groupDescription = Source.GroupDescription!;
      object groupKey = GetGroupKey(item);
      var groups = _collectionGroups.Cast<CollectionViewGroup>();
      CollectionViewGroup? group = groups.FirstOrDefault(grp => grp.Group.Equals(groupKey));
      if (group is null && createIfNotExists)
      {
        group = new() { Group = groupKey };
        if (groupDescription.SortDescription is not null)
        {
          int groupSortIndex = ~groups.ToList().BinarySearch(group, groupDescription.SortDescription.Comparer);
          Debug.WriteLine($"Group Sort Index: {groupSortIndex}, GroupKey: {groupKey}");
          _collectionGroups.Insert(groupSortIndex, group);
        }
        else
        {
          _collectionGroups.Add(group);
        }
      }
      return group ?? throw new ArgumentException("Group Not Found");
    }

    private int GetGroupFirstIndex(CollectionViewGroup targetGroup)
    {
      int index = 0;
      foreach (var group in _collectionGroups.Cast<CollectionViewGroup>())
      {
        if (group == targetGroup)
          break;
        index += group.GroupItems.Count;
      }
      return index;
    }

    public int GetIndexForGroupedView(T item)
    {
      CollectionViewGroup group = GetGroup(item);

      int index = GetGroupFirstIndex(group);
      foreach (T groupItem in group.GroupItems.OfType<T>())
      {
        if (item.Equals(groupItem))
          break;
        index++;
      }
      return index;
    }

    public object? GetItemFromGroupedViewIndex(int index)
    {
      foreach (CollectionViewGroup group in _collectionGroups)
      {
        index -= group.GroupItems.Count;
        if (index < 0)
          return group.GroupItems[~index];
      }
      return null;
    }

    private int InsertIntoGroup(int viewIndex, T item)
    {
      CollectionViewGroup group = GetGroup(item, true);

      int groupViewIndex = GetGroupFirstIndex(group);
      int groupItemIndex = 0;
      foreach (T groupItem in group.GroupItems.OfType<T>())
      {
        if (View.IndexOf(groupItem) < viewIndex)
        {
          groupViewIndex++;
          groupItemIndex++;
        }
        else
          break;
      }
      group.GroupItems.Insert(groupItemIndex, item);
      return groupViewIndex;
    }

    private int RemoveFromGroup(T item)
    {
      CollectionViewGroup group = GetGroup(item);

      int groupViewIndex = GetGroupFirstIndex(group);
      int groupItemIndex = group.GroupItems.IndexOf(item);
      group.GroupItems.RemoveAt(groupItemIndex);
      return groupViewIndex + groupItemIndex;
    }

    private bool ReplaceInGroup(T oldItem, T newItem, out int oldViewIndex, out int newViewIndex)
    {
      oldViewIndex = -1; newViewIndex = -1;
      object? groupKeyOld = GetGroupKey(oldItem);
      object? groupKeyNew = GetGroupKey(newItem);
      if (groupKeyOld is not null && groupKeyNew is not null)
      {
        var groups = _collectionGroups.Cast<CollectionViewGroup>();
        CollectionViewGroup oldGroup = GetGroup(oldItem);

        if (groupKeyOld.Equals(groupKeyNew))
        {
          oldViewIndex = oldGroup.GroupItems.IndexOf(oldItem);
          newViewIndex = oldViewIndex;
          oldGroup.GroupItems[newViewIndex] = newItem;
          return true;
        }
        else
        {
          CollectionViewGroup newGroup = GetGroup(newItem);

          oldViewIndex = oldGroup.GroupItems.IndexOf(oldItem);
          oldGroup.GroupItems.RemoveAt(oldViewIndex);
          oldViewIndex += GetGroupFirstIndex(oldGroup);

          if (Source.IsSourceSorted)
            newViewIndex = InsertIntoGroup(GetSortedViewIndex(newItem), newItem);
          else
          {
            newViewIndex = newGroup.GroupItems.Count;
            newGroup.GroupItems.Insert(newViewIndex, newItem);
            newViewIndex += GetGroupFirstIndex(newGroup);
          }
          return false;
        }
      }
      return true;
    }
    #endregion

    #region Current Item
    private bool MoveCurrentToIndex(int index)
    {
      if (index < -1 || index >= View.Count || index == CurrentPosition)
        return false;

      CurrentChangingEventArgs e = new();
      CurrentChanging?.Invoke(this, e);
      if (e.Cancel)
        return false;

      if (Source.IsSourceGrouped)
        CurrentItem = GetItemFromGroupedViewIndex(index);
      else
        CurrentItem = View[index];

      CurrentChanged?.Invoke(this, null!);
      return true;
    }

    public bool MoveCurrentTo(object item)
    {
      if (CurrentItem?.Equals(item) ?? false)
        return true;

      CurrentChangingEventArgs e = new();
      CurrentChanging?.Invoke(this, e);
      if (e.Cancel)
        return false;

      CurrentItem = item;
      CurrentChanged?.Invoke(this, null!);
      return true;
    }

    public bool MoveCurrentToPosition(int index) => MoveCurrentToIndex(index);

    public bool MoveCurrentToFirst() => MoveCurrentToIndex(0);

    public bool MoveCurrentToLast() => MoveCurrentToIndex(View.Count - 1);

    public bool MoveCurrentToNext() => MoveCurrentToIndex(CurrentPosition + 1);

    public bool MoveCurrentToPrevious() => MoveCurrentToIndex(CurrentPosition - 1);

    public IAsyncOperation<LoadMoreItemsResult>? LoadMoreItemsAsync(uint count)
    {
      var sil = Source as ISupportIncrementalLoading;
      return sil?.LoadMoreItemsAsync(count);
    }

    private object? _currentItem;
    public object? CurrentItem
    {
      get => _currentItem;
      set
      {
        _currentItem = value;
        if (value is null)
          CurrentPosition = -1;
        else
        {
          if (Source.IsSourceGrouped)
            CurrentPosition = GetIndexForGroupedView((T)value);
          else
            CurrentPosition = View.IndexOf((T)value);
        }
      }
    }

    public int CurrentPosition { get; private set; }

    public bool HasMoreItems => (Source as ISupportIncrementalLoading)?.HasMoreItems ?? false;

    public bool IsCurrentAfterLast => CurrentPosition >= View.Count;

    public bool IsCurrentBeforeFirst => CurrentPosition < 0;
    #endregion

    #region Events and Invocation
    public event EventHandler<object>? CurrentChanged;
    public event CurrentChangingEventHandler? CurrentChanging;
    public event VectorChangedEventHandler<object>? VectorChanged;

    private void OnVectorChanged(CollectionChange collectionChange, object? item = null, int index = -1)
      => VectorChanged?.Invoke(this, new VectorChangedEventArgs(collectionChange, item, index));

    #endregion

    #region IEnumerable Implementation
    public int IndexOf(object item)
    {
      if (item is null)
        return -1;
      if (Source.IsSourceGrouped)
        return GetIndexForGroupedView((T)item);
      else
        return View.IndexOf((T)item);
    }

    public void Insert(int index, object item)
    {
      if (!IsReadOnly)
        Source.Insert(index, (T)item);
    }

    public void RemoveAt(int index) => Remove(View[index]);

    public object this[int index]
    {
      get
      {
        if (Source.IsSourceGrouped)
          return GetItemFromGroupedViewIndex(index);
        else
          return View[index];
      }
      set => throw new NotSupportedException("Changing items in the View is not supported. Changing items in Source will be reflected in View.");
    }

    public void Add(object item)
    {
      if (!IsReadOnly)
        Source.Add((T)item);
    }

    public void Clear()
    {
      if (!IsReadOnly)
        Source.Clear();
    }

    public bool Contains(object item) => View.Contains((T)item);

    public void CopyTo(object[] array, int arrayIndex) => View.CopyTo([.. array.Cast<T>()], arrayIndex);

    public bool Remove(object item)
    {
      if (IsReadOnly)
        return false;

      Source.Remove((T)item);
      return true;
    }

    public int Count => View.Count;

    public bool IsReadOnly => Source == null || Source.IsReadOnly;

    public IEnumerator<object> GetEnumerator() => View.Cast<object>().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion
  }
  #endregion
}
#endregion
