namespace MyNotes.Contracts.ViewModel;

public interface IViewModelFactory<TViewModel>
{
  TViewModel Create();
}
