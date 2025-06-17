using MyNotes.Core.Dto;
using MyNotes.Core.Model;

namespace MyNotes.Core.Service;
internal class TagService
{
  public TagService(DatabaseService databaseService)
  {
    _databaseService = databaseService;
    LoadAllTags();
  }

  private readonly DatabaseService _databaseService;

  private readonly Dictionary<TagId, Tag> _cache = new();
  public IEnumerable<Tag> Tags => _cache.Values;

  private void AddToCache(Tag tag) => _cache.TryAdd(tag.Id, tag);

  private void RemoveFromCache(Tag tag) => _cache.Remove(tag.Id);

  private Tag ToTag(TagDbDto dto)
  {
    if (_cache.TryGetValue(new TagId(dto.Id), out Tag? tag))
      return tag;
    else
    {
      Tag newTag = new(new TagId(dto.Id), dto.Text, (TagColor)dto.Color);
      AddToCache(newTag);
      return newTag;
    }
  }

  private TagDbDto ToDto(Tag tag) => new() { Id = tag.Id.Value, Text = tag.Text, Color = (int)tag.Color };

  public IEnumerable<Tag> GetTags(NoteId noteId) 
    => _databaseService.GetTags(new GetNoteTagsDbDto() { NoteId = noteId.Value }).Result.Select(ToTag);

  public bool AddTagToNote(Note note, Tag tag) 
    => _databaseService.AddTagToNote(new TagToNoteDbDto() { NoteId = note.Id.Value, TagId = tag.Id.Value });

  public bool DeleteTagFromNote(Note note, Tag tag)
    => _databaseService.DeleteTagFromNote(new TagToNoteDbDto() { NoteId = note.Id.Value, TagId = tag.Id.Value });

  public void LoadAllTags()
  {
    //CachedTagDbDto cachedTagDbDto = new() { Ids = [.. _tags.Keys.Select(tag => tag.Value)] };
    //foreach (TagDbDto dto in _databaseService.GetUncachedTags(cachedTagDbDto).Result)
    //  ToTag(dto);

    foreach (TagDbDto dto in _databaseService.GetAllTags().Result)
      ToTag(dto);
  }

  public Tag CreateTag(string text, TagColor color)
  {
    Tag? tag = Tags.FirstOrDefault(tag => tag.Text == text);

    if (tag is not null)
      return tag;

    tag = new(new TagId(Guid.NewGuid()), text, color);
    AddToCache(tag);
    _databaseService.AddTag(ToDto(tag));
    return tag;
  }

  public void DeleteTag(Tag tag)
  {
    if (_databaseService.DeleteTag(new DeleteTagDbDto() { Id = tag.Id.Value }))
      RemoveFromCache(tag);
  }
}
