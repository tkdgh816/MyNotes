using MyNotes.Contracts.ViewModel;

namespace MyNotes.Core.ViewModel;

internal abstract class ViewModelFactoryBase<TModel, TViewModel> : IViewModelFactory<TViewModel> where TModel : class where TViewModel : class, IViewModel
{
  public readonly Dictionary<TModel, WeakReference<TViewModel>> _cache = new();

  public abstract TViewModel Create(TModel model);

  public virtual TViewModel? Get(TModel model) 
    => _cache.TryGetValue(model, out var wr) && wr.TryGetTarget(out var vm) ? vm : null;

  public bool Close(TModel model) => _cache.Remove(model);

  TViewModel IViewModelFactory<TViewModel>.Create() => throw new NotSupportedException();

}
