using MyNotes.Core.Models;
using MyNotes.Core.Services;

namespace MyNotes.Core.ViewModels;

public class TagsViewModel : ViewModelBase
{
  public TagsViewModel(DatabaseService databaseService)
  {
    DatabaseService = databaseService;
    foreach (string tag in DatabaseService.GetTagsAll())
      TagGroup.AddItem(tag);
    TagsCollectionViewSource.Source = TagGroup;
  }

  private readonly DatabaseService DatabaseService;
  public CollectionViewSource TagsCollectionViewSource { get; } = new() { IsSourceGrouped = true };
  public TagGroup TagGroup { get; } = new();

  public void AddTag(string tag)
  {
    tag = tag.Trim();
    if (string.IsNullOrWhiteSpace(tag) || TagGroup.Contains(tag))
      return;
    else if(DatabaseService.AddTag(tag))
      TagGroup.AddItem(tag);
  }

  public void DeleteTag(string tag)
  {
    if (TagGroup.RemoveItem(tag))
      DatabaseService.DeleteTag(tag);
  }

  private bool _isInclusionModeIntersect = true;
  public bool IsIntersectSelection
  {
    get => _isInclusionModeIntersect;
    set => SetProperty(ref _isInclusionModeIntersect, value);
  }
}
