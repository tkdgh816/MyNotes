namespace MyNotes.Common.Collections;

public class CollectionViewGroup : ICollectionViewGroup
{
  public required object Group { get; set; }
  public IObservableVector<object> GroupItems { get; } = new ObservableVector<object>();
}