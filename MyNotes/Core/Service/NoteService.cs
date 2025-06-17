using MyNotes.Core.Dto;
using MyNotes.Core.Model;
using MyNotes.Debugging;

using ToolkitColorHelper = CommunityToolkit.WinUI.Helpers.ColorHelper;

namespace MyNotes.Core.Service;

internal class NoteService
{
  public NoteService(DatabaseService databaseService, TagService tagService)
  {
    _databaseService = databaseService;
    _tagService = tagService;
  }

  private readonly DatabaseService _databaseService;
  private readonly TagService _tagService;
  private readonly Dictionary<NoteId, Note> _cache = new();
  public IEnumerable<Note> Notes => _cache.Values;

  public void RemoveFromCache(Note note) => _cache.Remove(note.Id);
  private Note ToNote(NoteDbDto dto)
  {
    NoteId noteId = new(dto.Id);
    if (_cache.TryGetValue(noteId, out Note? note))
      return note;
    else
    {
      Note newNote = new(noteId, new BoardId(dto.Parent), dto.Title, dto.Created, dto.Modified)
      {
        Body = dto.Body,
        Background = ToolkitColorHelper.ToColor(dto.Background),
        Backdrop = (BackdropKind)dto.Backdrop,
        Size = new SizeInt32(dto.Width, dto.Height),
        Position = new PointInt32(dto.PositionX, dto.PositionY),
        IsBookmarked = dto.Bookmarked,
        IsTrashed = dto.Trashed,
        Tags = new(_tagService.GetTags(noteId))
      };
      newNote.Initialize();
      _cache.Add(newNote.Id, newNote);
      ReferenceTracker.NoteReferences.Add(new(newNote));
      return newNote;
    }
  }
  private NoteDbDto ToDto(Note note) => new() { Id = note.Id.Value, Parent = note.BoardId.Value, Created = note.Created, Modified = note.Modified, Title = note.Title, Body = note.Body, Background = note.Background.ToString(), Backdrop = (int)note.Backdrop, Width = note.Size.Width, Height = note.Size.Height, PositionX = note.Position.X, PositionY = note.Position.Y, Bookmarked = note.IsBookmarked, Trashed = note.IsTrashed };

  public Note CreateNote(NavigationUserBoard navigation)
  {
    DateTimeOffset creationTime = DateTimeOffset.UtcNow;
    Note newNote = new(new NoteId(Guid.NewGuid()), navigation.Id, "New note", creationTime, creationTime) { Background = Colors.LightGoldenrodYellow, Position = new(-1, -1) };
    _databaseService.AddNote(ToDto(newNote));
    _cache.Add(newNote.Id, newNote);
    return newNote;
  }
  public IEnumerable<Note> GetNotes(NavigationUserBoard navigation)
    => _databaseService.GetNotes(new GetBoardNotesDbDto() { Id = navigation.Id.Value }).Result.Select(ToNote);
  public IEnumerable<Note> GetBookmarkedNotes()
    => _databaseService.GetBookmarkedNotes().Result.Select(ToNote);
  public IEnumerable<Note> GetTrashedNotes()
    => _databaseService.GetTrashedNotes().Result.Select(ToNote);

  public void UpdateNote(Note note, NoteUpdateFields updateFields)
  {
    if (updateFields == NoteUpdateFields.None)
      return;

    UpdateNoteDbDto dto = new()
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
    _databaseService.UpdateNote(dto);
  }
}
