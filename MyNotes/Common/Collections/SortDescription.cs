namespace MyNotes.Common.Collections;

internal class SortDescription<T> : INotifyPropertyChanged
{
  public IComparer<T> Comparer { get; private set; }

  private SortDirection _direction;
  public SortDirection Direction
  {
    get => _direction;
    set
    {
      _direction = value;
      if (value is SortDirection.Descending)
        Comparer = new ReverseComparer(Comparer);
      
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Direction)));
    }
  }

  public string? KeyPropertyName { get; set; }

  public SortDescription(IComparer<T> comparer, SortDirection direction = SortDirection.Ascending, string? keyPropertyName = null)
  {
    Comparer = comparer;
    Direction = direction;
    KeyPropertyName = keyPropertyName;
  }

  public SortDescription(Func<T, IComparable> func, SortDirection direction = SortDirection.Ascending, string? keyPropertyName = null)
  {
    Comparer = new FuncComparer() { CompareFunc = func };
    Direction = direction;
    KeyPropertyName = keyPropertyName;
  }

  private class ReverseComparer : IComparer<T>
  {
    private IComparer<T> _comparer;
    public ReverseComparer(IComparer<T> comparer) => _comparer = comparer;

    public int Compare(T? x, T? y) => _comparer.Compare(y, x);
  }

  private class FuncComparer : IComparer<T>
  {
    public required Func<T, IComparable> CompareFunc { get; init; }
    public int Compare(T? x, T? y)
    {
      if (x is null || y is null)
        return 0;
      return CompareFunc(x).CompareTo(CompareFunc(y));
    }
  }

  public event PropertyChangedEventHandler? PropertyChanged;
}

internal enum SortDirection { Ascending, Descending }