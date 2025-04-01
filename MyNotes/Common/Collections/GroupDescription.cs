namespace MyNotes.Common.Collections;

public class GroupDescription<T>
{
  public string? KeyProperty { get; init; } = null;
  public PropertyInfo? KeyPropertyInfo;
  public Func<T, object>? KeySelector { get; init; } = null;
  public SortDescription<ICollectionViewGroup>? SortDescription { get; set; }

  public GroupDescription(string keyProperty, SortDescription<object>? groupSortDescription = null)
  {
    KeyPropertyInfo = typeof(T).GetProperty(keyProperty) ?? throw new ArgumentException("No such property");
    KeyProperty = keyProperty;
    if (groupSortDescription is not null)
      SortDescription = new(new GroupComparer(groupSortDescription.Comparer));
  }

  public GroupDescription(Func<T, object> keySelector, SortDescription<object>? groupSortDescription = null)
  {
    KeySelector = keySelector;
    if (groupSortDescription is not null)
      SortDescription = new(new GroupComparer(groupSortDescription.Comparer));
  }

  private class GroupComparer : IComparer<ICollectionViewGroup>
  {
    private IComparer<object> _comparer;
    public GroupComparer(IComparer<object> comparer) => _comparer = comparer;

    public int Compare(ICollectionViewGroup? x, ICollectionViewGroup? y)
    {
      if (x is null && y is null)
        return 0;
      else if (x is null)
        return -1;
      else if (y is null)
        return 1;
      else
        return _comparer.Compare(x.Group, y.Group);
    }
  }
}
