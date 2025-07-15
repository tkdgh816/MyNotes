using Microsoft.Extensions.DependencyInjection;

using MyNotes.Core.Dao;
using MyNotes.Core.Dto;
using MyNotes.Core.Model;

namespace MyNotes.Core.Service;
internal class TagService
{
  public TagService([FromKeyedServices("TagDbDao")] DbDaoBase dbDaoBase)
  {
    _tagDbDao = (TagDbDao)dbDaoBase;
    LoadAllTags();
  }

  private readonly TagDbDao _tagDbDao;

  private readonly Dictionary<TagId, Tag> _cache = new();
  public IEnumerable<Tag> Tags => _cache.Values;

  private void AddToCache(Tag tag) => _cache.TryAdd(tag.Id, tag);

  private void RemoveFromCache(Tag tag) => _cache.Remove(tag.Id);

  private Tag ToTag(TagDto dto)
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

  private TagDto ToDto(Tag tag) => new() { Id = tag.Id.Value, Text = tag.Text, Color = (int)tag.Color };

  public IEnumerable<Tag> GetTags(NoteId noteId) 
    => _tagDbDao.GetTags(new GetNoteTagsDto() { NoteId = noteId.Value }).Result.Select(ToTag);

  public bool AddTagToNote(Note note, Tag tag) 
    => _tagDbDao.AddTagToNote(new TagToNoteDto() { NoteId = note.Id.Value, TagId = tag.Id.Value });

  public bool DeleteTagFromNote(Note note, Tag tag)
    => _tagDbDao.DeleteTagFromNote(new TagToNoteDto() { NoteId = note.Id.Value, TagId = tag.Id.Value });

  public void LoadAllTags()
  {
    //CachedTagDbDto cachedTagDbDto = new() { Ids = [.. _tags.Keys.Select(tag => tag.Value)] };
    //foreach (TagDbDto dto in _tagDao.GetUncachedTags(cachedTagDbDto).Result)
    //  ToTag(dto);

    foreach (TagDto dto in _tagDbDao.GetAllTags().Result)
      ToTag(dto);
  }

  public Tag CreateTag(string text, TagColor color)
  {
    Tag? tag = Tags.FirstOrDefault(tag => tag.Text == text);

    if (tag is not null)
      return tag;

    tag = new(new TagId(Guid.NewGuid()), text, color);
    AddToCache(tag);
    _tagDbDao.AddTag(ToDto(tag));
    return tag;
  }

  public void DeleteTag(Tag tag)
  {
    if (_tagDbDao.DeleteTag(new DeleteTagDto() { Id = tag.Id.Value }))
      RemoveFromCache(tag);
  }
}
