using MyNotes.Common.Commands;
using MyNotes.Core.Dto;
using MyNotes.Core.Model;

namespace MyNotes.Core.ViewModel;
internal partial class TagsViewModel : ViewModelBase
{
  public Command<string>? SearchTagCommand { get; private set; }
  public Command<string>? ResetSearchCommand { get; private set; }
  public Command<TagCommandParameterDto>? AddTagCommand { get; private set; }
  public Command? ExploreTagsCommand { get; private set; }
  public Command? DeleteTagsCommand { get; private set; }
  public Command? ToggleInclusionCommand { get; private set; }

  private void SetCommands()
  {
    SearchTagCommand = new((queryText) =>
    {
      if (string.IsNullOrWhiteSpace(queryText))
      {
        TagsCollectionViewSource.Source = TagGroup;
        _isSearchingState = false;
      }
      else
      {
        TagsCollectionViewSource.Source = TagGroup.Select(group => new Tags(group.Key, group.Where(tag => tag.Text.Contains(queryText, StringComparison.CurrentCultureIgnoreCase))));
        _isSearchingState = true;
      }
    });

    ResetSearchCommand = new((queryText) =>
    {
      if (_isSearchingState && string.IsNullOrWhiteSpace(queryText))
      {
        TagsCollectionViewSource.Source = TagGroup;
        _isSearchingState = false;
      }
    });

    AddTagCommand = new((parameter) =>
    {
      string tagText = parameter.Text.Trim();
      if (string.IsNullOrWhiteSpace(tagText) || TagGroup.Contains(tagText))
        return;
      Tag newTag = _tagService.CreateTag(tagText, parameter.Color);
      TagGroup.AddItem(newTag);
    });

    ExploreTagsCommand = new(
      () => Debug.WriteLine(string.Join(", ", SelectedTags.Select(tag => tag.Text))),
      () => SelectedTags.Any());

    DeleteTagsCommand = new(() =>
    {
      foreach (Tag tag in SelectedTags)
        DeleteTag(tag);
    });

    ToggleInclusionCommand = new(() => IsIntersectSelection = !IsIntersectSelection);
  }
}
