using Microsoft.Extensions.DependencyInjection;

using MyNotes.Core.Dao;
using MyNotes.Core.Dto;
using MyNotes.Core.Model;
using MyNotes.Debugging;

using ToolkitColorHelper = CommunityToolkit.WinUI.Helpers.ColorHelper;

namespace MyNotes.Core.Service;

internal class NoteService([FromKeyedServices("NoteDao")] DbDaoBase daoBase, TagService tagService, SettingsService settingsService)
{
  private readonly NoteDbDao _noteDao = (NoteDbDao)daoBase;
  private readonly TagService _tagService = tagService;
  private readonly SettingsService _settingsService = settingsService;
  private readonly StorageFolder _noteFolder = ApplicationData.Current.LocalFolder.CreateFolderAsync("notes", CreationCollisionOption.OpenIfExists).GetAwaiter().GetResult();

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
  private NoteDto ToDto(Note note) => new() { Id = note.Id.Value, Parent = note.BoardId.Value, Created = note.Created, Modified = note.Modified, Title = note.Title, Body = note.Body, Background = note.Background.ToString(), Backdrop = (int)note.Backdrop, Width = note.Size.Width, Height = note.Size.Height, PositionX = note.Position.X, PositionY = note.Position.Y, Bookmarked = note.IsBookmarked, Trashed = note.IsTrashed };

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
    _noteDao.AddNote(ToDto(newNote));
    return newNote;
  }

  public bool DeleteNote(Note note) => _noteDao.DeleteNote(new DeleteNoteDto() { Id = note.Id.Value });

  public Note? GetNote(NoteId noteId) => _cache.TryGetValue(noteId, out Note? note) ? note : null;

  public IEnumerable<Note> GetNotes(NavigationUserBoard navigation)
    => _noteDao.GetNotes(new GetBoardNotesDto() { Id = navigation.Id.Value }).Result.Select(ToNote);
  public IEnumerable<Note> GetBookmarkedNotes()
    => _noteDao.GetBookmarkedNotes().Result.Select(ToNote);
  public IEnumerable<Note> GetTrashedNotes()
    => _noteDao.GetTrashedNotes().Result.Select(ToNote);

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
    _noteDao.UpdateNote(dto);
  }

  public Task<StorageFile> GetFile(Note note)
    => _noteFolder.GetFileAsync($"{note.Id.Value}.rtf").AsTask();

  public Task<StorageFile> CreateFile(Note note) 
    => _noteFolder.CreateFileAsync($"{note.Id.Value}.rtf", CreationCollisionOption.OpenIfExists).AsTask();

  public async Task DeleteFile(Note note)
  {
    try
    {
      await (await CreateFile(note)).DeleteAsync(StorageDeleteOption.PermanentDelete);
    }
    catch (Exception)
    { }
  }
}
