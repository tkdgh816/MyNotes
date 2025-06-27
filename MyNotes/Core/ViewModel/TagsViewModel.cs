using MyNotes.Common.Commands;
using MyNotes.Common.Messaging;
using MyNotes.Core.Dto;
using MyNotes.Core.Model;
using MyNotes.Core.Service;
using MyNotes.Core.Shared;

namespace MyNotes.Core.ViewModel;

internal class TagsViewModel : ViewModelBase
{
  private readonly TagService _tagService;

  public TagsViewModel(TagService tagService)
  {
    _tagService = tagService;
    TagsCollectionViewSource.Source = TagGroup;

    SetCommands();
    RegisterMessengers();
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing)
    {
      UnregisterMessengers();
    }
    base.Dispose(disposing);
  }
  public CollectionViewSource TagsCollectionViewSource { get; } = new() { IsSourceGrouped = true };
  public TagGroup TagGroup { get; } = new();

  public void RefreshTagGroup()
  {
    TagGroup.Clear();
    foreach (Tag tag in _tagService.Tags)
      TagGroup.AddItem(tag);
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

  private bool _isSearchingState = false;

  public ObservableCollection<Tag> SelectedTags { get; } = new();

  #region Commands
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
  #endregion

  #region Messengers
  private void RegisterMessengers()
  {
    WeakReferenceMessenger.Default.Register<Message, string>(this, Tokens.RefreshTagGroup, new((recipient, message) => RefreshTagGroup()));
  }

  private void UnregisterMessengers() => WeakReferenceMessenger.Default.UnregisterAll(this);
  #endregion
}
