namespace MyNotes.Core.ViewModels;

public class ViewModelBase : ObservableObject, IDisposable
{
  public virtual void Dispose()
  { }
}
