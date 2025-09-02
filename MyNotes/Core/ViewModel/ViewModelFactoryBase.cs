using MyNotes.Contracts.ViewModel;

namespace MyNotes.Core.ViewModel;

internal abstract class ViewModelFactoryBase<TModel, TViewModel> : IViewModelFactory<TViewModel> where TModel : class where TViewModel : class, IViewModel
{
  protected readonly Dictionary<TModel, WeakReference<TViewModel>> _cache = new();

  public abstract TViewModel Resolve(TModel model);

  public TViewModel? Get(TModel model) 
    => _cache.TryGetValue(model, out var wr) && wr.TryGetTarget(out var vm) ? vm : null;

  public bool Remove(TModel model) => _cache.Remove(model);

  TViewModel IViewModelFactory<TViewModel>.Resolve() => throw new NotSupportedException();
}
