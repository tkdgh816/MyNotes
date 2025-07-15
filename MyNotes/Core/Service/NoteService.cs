using Microsoft.Extensions.DependencyInjection;

using MyNotes.Core.Dao;
using MyNotes.Core.Dto;
using MyNotes.Core.Model;
using MyNotes.Debugging;

using ToolkitColorHelper = CommunityToolkit.WinUI.Helpers.ColorHelper;

namespace MyNotes.Core.Service;

internal class NoteService([FromKeyedServices("NoteDbDao")] DbDaoBase dbDaoBase, NoteFileDao noteFileDao, NoteSearchDao noteSearchDao, TagService tagService, SettingsService settingsService)
{
  private readonly NoteDbDao _dbDao = (NoteDbDao)dbDaoBase;
  private readonly NoteFileDao _fileDao = noteFileDao;
  private readonly NoteSearchDao _searchDao = noteSearchDao;
  private readonly TagService _tagService = tagService;
  private readonly SettingsService _settingsService = settingsService;

  private readonly Dictionary<NoteId, Note> _cache = new();
  public IEnumerable<Note> Notes => _cache.Values;

  public void AddToCache(Note note)
  {
    if (_cache.TryAdd(note.Id, note))
      note.Initialize();
  }
  public void RemoveFromCache(Note note) => _cache.Remove(note.Id);
  private Note ToNote(NoteDto dto)
  {
    NoteId noteId = new(dto.Id);
    if (_cache.TryGetValue(noteId, out Note? note))
      return note;
    else
    {
      Note newNote = new(dto.Title, dto.Modified)
      {
        Id = noteId,
        BoardId = new BoardId(dto.Parent),
        Created = dto.Created,
        Body = dto.Body,
        Background = ToolkitColorHelper.ToColor(dto.Background),
        Backdrop = (BackdropKind)dto.Backdrop,
        Size = new SizeInt32(dto.Width, dto.Height),
        Position = new PointInt32(dto.PositionX, dto.PositionY),
        IsBookmarked = dto.Bookmarked,
        IsTrashed = dto.Trashed,
        Tags = new(_tagService.GetTags(noteId))
      };
      AddToCache(newNote);
      ReferenceTracker.NoteReferences.Add(new(newNote.Title, newNote));
      return newNote;
    }
  }
  private NoteDto ToNoteDto(Note note) => new()
  {
    Id = note.Id.Value,
    Parent = note.BoardId.Value,
    Created = note.Created,
    Modified = note.Modified,
    Title = note.Title,
    Body = note.Body,
    Background = note.Background.ToString(),
    Backdrop = (int)note.Backdrop,
    Width = note.Size.Width,
    Height = note.Size.Height,
    PositionX = note.Position.X,
    PositionY = note.Position.Y,
    Bookmarked = note.IsBookmarked,
    Trashed = note.IsTrashed
  };

  public Note CreateNote(NavigationUserBoard navigation)
  {
    DateTimeOffset creationTime = DateTimeOffset.UtcNow;
    var settings = _settingsService.GetNoteSettings();
    Note newNote = new("New note", creationTime)
    {
      Id = new NoteId(Guid.NewGuid()),
      BoardId = navigation.Id,
      Created = creationTime,
      Background = settings.Background,
      Backdrop = settings.Backdrop,
      Size = settings.Size,
      Position = new(-1, -1)
    };
    AddToCache(newNote);
    _dbDao.AddNote(ToNoteDto(newNote));
    _searchDao.AddSearchDocument(new NoteSearchDto() { Id = newNote.Id.Value, Title = newNote.Title, Body = newNote.Body });
    return newNote;
  }

  public void DeleteNote(Note note)
  {
    DeleteNoteDto dto = new() { Id = note.Id.Value };
    _dbDao.DeleteNote(dto);
    _searchDao.DeleteSearchDocument(dto);
  }

  public Note? GetNote(NoteId noteId) => _cache.TryGetValue(noteId, out Note? note) ? note : null;

  public IEnumerable<Note> GetNotes(NavigationUserBoard navigation)
    => _dbDao.GetNotes(new GetBoardNotesDto() { Id = navigation.Id.Value }).Result.Select(ToNote);
  public IEnumerable<Note> GetBookmarkedNotes()
    => _dbDao.GetBookmarkedNotes().Result.Select(ToNote);
  public IEnumerable<Note> GetTrashedNotes()
    => _dbDao.GetTrashedNotes().Result.Select(ToNote);
  public IEnumerable<Note> SearchNotes(string searchText)
  {
    List<GetNoteDto> dtos = new(_searchDao.GetNoteSearchIds(searchText));
    return _dbDao.SearchNotes(dtos).Result.Select(ToNote);
  }

  public void UpdateNote(Note note, NoteUpdateFields updateFields)
  {
    if (updateFields == NoteUpdateFields.None)
      return;

    UpdateNoteDto dto = new()
    {
      UpdateFields = updateFields,
      Id = note.Id.Value,
      Parent = updateFields.HasFlag(NoteUpdateFields.Parent) ? note.BoardId.Value : null,
      Modified = updateFields.HasFlag(NoteUpdateFields.Modified) ? note.Modified : null,
      Title = updateFields.HasFlag(NoteUpdateFields.Title) ? note.Title : null,
      Body = updateFields.HasFlag(NoteUpdateFields.Body) ? note.Body : null,
      Background = updateFields.HasFlag(NoteUpdateFields.Background) ? note.Background.ToString() : null,
      Backdrop = updateFields.HasFlag(NoteUpdateFields.Backdrop) ? (int)note.Backdrop : null,
      Width = updateFields.HasFlag(NoteUpdateFields.Width) ? note.Size.Width : null,
      Height = updateFields.HasFlag(NoteUpdateFields.Height) ? note.Size.Height : null,
      PositionX = updateFields.HasFlag(NoteUpdateFields.PositionX) ? note.Position.X : null,
      PositionY = updateFields.HasFlag(NoteUpdateFields.PositionY) ? note.Position.Y : null,
      Bookmarked = updateFields.HasFlag(NoteUpdateFields.Bookmarked) ? note.IsBookmarked : null,
      Trashed = updateFields.HasFlag(NoteUpdateFields.Trashed) ? note.IsTrashed : null
    };
    _dbDao.UpdateNote(dto);
  }

  public void UpdateSearchDocument(Note note, string body)
  {
    NoteSearchDto dto = new()
    {
      Id = note.Id.Value,
      Title = note.Title,
      Body = body
    };
    _searchDao.UpdateNoteSearchDocument(dto);
  }

  public Task<StorageFile> GetFile(Note note)
    => _fileDao.GetFile(new NoteFileDto() { FileName = $"{note.Id.Value}.rtf" });

  public Task<StorageFile> CreateFile(Note note)
    => _fileDao.CreateFile(new NoteFileDto() { FileName = $"{note.Id.Value}.rtf" });

  public Task DeleteFile(Note note)
    => _fileDao.DeleteFile(new NoteFileDto() { FileName = $"{note.Id.Value}.rtf" });
}
