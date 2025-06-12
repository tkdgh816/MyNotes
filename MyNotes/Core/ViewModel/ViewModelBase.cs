using MyNotes.Contracts.ViewModel;

namespace MyNotes.Core.ViewModel;

internal abstract class ViewModelBase : ObservableObject, IViewModel
{
  protected bool Disposed { get; private set; } = false;

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (Disposed)
      return;

    if(disposing) { }

    Disposed = true;
  }
}
