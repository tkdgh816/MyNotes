namespace MyNotes.Common.Collections;

public class SortDescription<T> : INotifyPropertyChanged
{
  private SortDirection _direction;
  public SortDirection Direction
  {
    get => _direction;
    set
    {
      _direction = value;
      if (value is SortDirection.Descending)
      {
        Comparer = new ReverseComparer(Comparer);
        Debug.WriteLine("Reverse Comparer");
      }
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Direction)));
    }
  }

  public IComparer<T> Comparer { get; private set; }

  public SortDescription(IComparer<T> comparer, SortDirection direction = SortDirection.Ascending)
  {
    Comparer = comparer;
    Direction = direction;
  }

  public SortDescription(Func<T, IComparable> func, SortDirection direction = SortDirection.Ascending)
  {
    Comparer = new FuncComparer() { CompareFunc = func };
    Direction = direction;
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

public enum SortDirection { Ascending, Descending }