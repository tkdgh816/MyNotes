using MyNotes.Contracts.ViewModel;

namespace MyNotes.Core.ViewModel;

internal abstract class ViewModelBase : ObservableObject, IViewModel
{
  private bool _disposed = false;

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (_disposed)
      return;

    if(disposing) { }

    _disposed = true;
  }
}
