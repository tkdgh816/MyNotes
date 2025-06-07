using MyNotes.Core.Model;
using MyNotes.Core.Service;

namespace MyNotes.Core.ViewModel;

internal class TagsViewModel : ViewModelBase
{
  public TagsViewModel(DatabaseService databaseService)
  {
    _databaseService = databaseService;
    foreach (string tag in _databaseService.GetTagsAll().Result)
      TagGroup.AddItem(tag);
    TagsCollectionViewSource.Source = TagGroup;
  }

  private readonly DatabaseService _databaseService;
  public CollectionViewSource TagsCollectionViewSource { get; } = new() { IsSourceGrouped = true };
  public TagGroup TagGroup { get; } = new();

  public void AddTag(string tag)
  {
    tag = tag.Trim();
    if (string.IsNullOrWhiteSpace(tag) || TagGroup.Contains(tag))
      return;
    else if(_databaseService.AddTag(tag))
      TagGroup.AddItem(tag);
  }

  public void DeleteTag(string tag)
  {
    if (TagGroup.RemoveItem(tag))
      _databaseService.DeleteTag(tag);
  }

  private bool _isInclusionModeIntersect = true;
  public bool IsIntersectSelection
  {
    get => _isInclusionModeIntersect;
    set => SetProperty(ref _isInclusionModeIntersect, value);
  }
}
