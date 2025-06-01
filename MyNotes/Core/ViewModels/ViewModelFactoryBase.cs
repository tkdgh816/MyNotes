namespace MyNotes.Core.ViewModels;

internal abstract class ViewModelFactoryBase<TModel, TViewModel> where TModel : class where TViewModel : class
{
  public readonly Dictionary<TModel, WeakReference<TViewModel>> _cache = new();

  public abstract TViewModel Create(TModel model);

  public bool Dispose(TModel model) => _cache.Remove(model);
}
