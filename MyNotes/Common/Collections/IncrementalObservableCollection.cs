using System.Runtime.InteropServices.WindowsRuntime;

namespace MyNotes.Common.Collections;

public class IncrementalObservableCollection<T> : ObservableCollection<T>, ISupportIncrementalLoading
{
  private readonly Func<uint, Task<T[]>> _loadDataFunc;
  private readonly uint _pageSize;
  private uint _offset = 0;
  private bool _isLoading = false;

  public IncrementalObservableCollection(Func<uint, Task<T[]>> loadDataFunc, uint pageSize = 20)
  {
    _loadDataFunc = loadDataFunc ?? throw new ArgumentNullException(nameof(loadDataFunc));
    _pageSize = pageSize;
  }

  public bool HasMoreItems { get; private set; } = true;

  public bool IsLoading => _isLoading;

  public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
  {
    return AsyncInfo.Run(async cancellationToken =>
    {
      if (_isLoading || !HasMoreItems)
        return new LoadMoreItemsResult { Count = 0 };

      _isLoading = true;

      var items = await _loadDataFunc(_offset);

      foreach (var item in items)
      {
        Add(item);
      }

      _offset += (uint)items.Length;

      if (items.Length < _pageSize)
        HasMoreItems = false;

      _isLoading = false;

      return new LoadMoreItemsResult { Count = (uint)items.Length };
    });
  }
}
