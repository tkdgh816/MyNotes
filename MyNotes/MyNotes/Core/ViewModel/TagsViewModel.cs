using MyNotes.Common.Messaging;
using MyNotes.Core.Model;
using MyNotes.Core.Service;
using MyNotes.Core.Shared;

namespace MyNotes.Core.ViewModel;

internal partial class TagsViewModel : ViewModelBase
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

  #region Messengers
  private void RegisterMessengers()
  {
    WeakReferenceMessenger.Default.Register<Message, string>(this, Tokens.RefreshTagGroup, new((recipient, message) => RefreshTagGroup()));
  }

  private void UnregisterMessengers() => WeakReferenceMessenger.Default.UnregisterAll(this);
  #endregion
}
