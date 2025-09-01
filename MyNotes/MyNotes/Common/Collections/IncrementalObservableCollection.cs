using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;

namespace MyNotes.Common.Collections;

internal class IncrementalObservableCollection<T> : ObservableCollection<T>, ISupportIncrementalLoading
{
  private readonly Func<uint, Task<IEnumerable<T>>>? _loadItemsTaskFunc;
  private readonly Func<uint, IAsyncEnumerable<T>>? _loadItemsIAsyncEnumerableFunc;

  private bool _hasMoreItems = true;
  private bool _isLoading = false;
  private readonly object _lockObj = new();
  //private readonly Lock _lockObj = new();

  public event EventHandler<MoreItemsLoadedEventArgs>? MoreItemsLoaded;

  public IncrementalObservableCollection(Func<uint, Task<IEnumerable<T>>> loadItemsTaskFunc)
    => _loadItemsTaskFunc = loadItemsTaskFunc;

  public IncrementalObservableCollection(Func<uint, IAsyncEnumerable<T>> loadItemsIAsyncEnumerableFunc)
   => _loadItemsIAsyncEnumerableFunc = loadItemsIAsyncEnumerableFunc;

  public bool HasMoreItems => _hasMoreItems && !_isLoading;

  private async Task<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
  {
    lock (_lockObj)
    {
      if (_isLoading)
        return new LoadMoreItemsResult(0);

      _isLoading = true;
    }

    uint itemCount = 0;
    try
    {
      await Task.Delay(10);

      if (_loadItemsTaskFunc is not null)
      {
        var enumerator = (await _loadItemsTaskFunc(count)).GetEnumerator();

        while (enumerator.MoveNext())
        {
          itemCount++;
          Add(enumerator.Current);
        }
      }
      else if (_loadItemsIAsyncEnumerableFunc is not null)
      {
        await foreach (var item in _loadItemsIAsyncEnumerableFunc(count))
        {
          itemCount++;
          Add(item);
        }
      }
      else
        throw new ArgumentException("LoadItems Func is null");

      if (itemCount == 0)
        _hasMoreItems = false;
    }
    catch (Exception)
    { }

    _isLoading = false;
    MoreItemsLoaded?.Invoke(this, new MoreItemsLoadedEventArgs(itemCount));
    return new LoadMoreItemsResult(itemCount);
  }

  IAsyncOperation<LoadMoreItemsResult> ISupportIncrementalLoading.LoadMoreItemsAsync(uint count) => AsyncInfo.Run((c) => LoadMoreItemsAsync(count));

  protected override void ClearItems()
  {
    base.ClearItems();
    _hasMoreItems = true;
  }
}

internal class MoreItemsLoadedEventArgs(uint count) : EventArgs
{
  public uint Count { get; } = count;
}
