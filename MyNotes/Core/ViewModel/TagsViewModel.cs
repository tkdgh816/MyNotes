using MyNotes.Core.Model;
using MyNotes.Core.Service;

namespace MyNotes.Core.ViewModel;

internal class TagsViewModel : ViewModelBase
{
  public TagsViewModel(TagService tagService)
  {
    _tagService = tagService;
    foreach (Tag tag in _tagService.Tags)
      TagGroup.AddItem(tag);
    TagsCollectionViewSource.Source = TagGroup;
  }

  private readonly TagService _tagService;
  public CollectionViewSource TagsCollectionViewSource { get; } = new() { IsSourceGrouped = true };
  public TagGroup TagGroup { get; } = new();

  public void AddTag(string tagText)
  {
    tagText = tagText.Trim();
    if (string.IsNullOrWhiteSpace(tagText) || TagGroup.Contains(tagText))
      return;
    Tag newTag = _tagService.CreateTag(tagText);
    TagGroup.AddItem(newTag);
  }

  public void DeleteTag(Tag tag)
  {
    if (TagGroup.RemoveItem(tag))
      _tagService.DeleteTag(tag);
  }

  private bool _isInclusionModeIntersect = true;
  public bool IsIntersectSelection
  {
    get => _isInclusionModeIntersect;
    set => SetProperty(ref _isInclusionModeIntersect, value);
  }
}
