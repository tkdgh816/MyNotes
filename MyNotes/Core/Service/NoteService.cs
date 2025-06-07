using MyNotes.Common.Collections;
using MyNotes.Core.Dto;
using MyNotes.Core.Model;
using MyNotes.Core.ViewModel;
using MyNotes.Debugging;

using ToolkitColorHelper = CommunityToolkit.WinUI.Helpers.ColorHelper;

namespace MyNotes.Core.Service;

internal class NoteService
{
  public NoteService(DatabaseService databaseService, NoteViewModelFactory noteViewModelFactory)
  {
    _databaseService = databaseService;
    _noteViewModelFactory = noteViewModelFactory;
  }

  private readonly DatabaseService _databaseService;
  private readonly NoteViewModelFactory _noteViewModelFactory;
  private readonly Dictionary<Guid, Note> _notes = new();

  public void RemoveNoteCache(Note note) => _notes.Remove(note.Id);
  private Note ToNote(NoteDbDto dto)
  {
    if (_notes.TryGetValue(dto.Id, out Note? note))
      return note;
    else
    {
      Note newNote = new(dto.Id, dto.Parent, dto.Title, dto.Created, dto.Modified)
      {
        Body = dto.Body,
        Background = ToolkitColorHelper.ToColor(dto.Background),
        Backdrop = (BackdropKind)dto.Backdrop,
        Size = new SizeInt32(dto.Width, dto.Height),
        Position = new PointInt32(dto.PositionX, dto.PositionY),
        IsBookmarked = dto.Bookmarked,
        IsTrashed = dto.Trashed
      };
      newNote.Initialize();
      _notes.Add(newNote.Id, newNote);
      ReferenceTracker.NoteReferences.Add(new(newNote));
      return newNote;
    }
  }
  private NoteDbDto ToDto(Note note) => new() { Id = note.Id, Parent = note.BoardId, Created = note.Created, Modified = note.Modified, Title = note.Title, Body = note.Body, Background = note.Background.ToString(), Backdrop = (int)note.Backdrop, Width = note.Size.Width, Height = note.Size.Height, PositionX = note.Position.X, PositionY = note.Position.Y, Bookmarked = note.IsBookmarked, Trashed = note.IsTrashed, Tags = [.. note.Tags] };

  public (Note Note, NoteViewModel NoteViewModel) CreateNote(NavigationUserBoard navigation)
  {
    DateTimeOffset creationTime = DateTimeOffset.UtcNow;
    Note newNote = new(Guid.NewGuid(), navigation.Id, "New note", creationTime, creationTime) { Background = Colors.LightGoldenrodYellow };
    _databaseService.AddNote(ToDto(newNote));
    _notes.Add(newNote.Id, newNote);
    NoteViewModel noteViewModel = _noteViewModelFactory.Create(newNote);
    _currentNoteViewModels?.Add(noteViewModel);
    return (newNote, noteViewModel);
  }
  public IEnumerable<Note> GetNotes(NavigationUserBoard navigation)
    => _databaseService.GetNotes(navigation).Result.Select(ToNote);
  public IEnumerable<Note> GetBookmarkedNotes()
    => _databaseService.GetBookmarkedNotes().Result.Select(ToNote);
  public IEnumerable<Note> GetTrashedNotes()
    => _databaseService.GetTrashedNotes().Result.Select(ToNote);

  public void UpdateNote(Note note, NoteUpdateFields updateFields)
  {
    if (updateFields == NoteUpdateFields.None)
      return;

    NoteDbUpdateDto dto = new()
    {
      UpdateFields = updateFields,
      Id = note.Id,
      Parent = updateFields.HasFlag(NoteUpdateFields.Parent) ? note.BoardId : null,
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

  private SortedObservableCollection<NoteViewModel>? _currentNoteViewModels;

  public SortedObservableCollection<NoteViewModel> GetNoteViewModels(NavigationBoard navigation)
  {
    SortedObservableCollection<NoteViewModel> noteViewModels = navigation switch
    {
      NavigationUserBoard userBoard => new(GetNotes(userBoard).Select(_noteViewModelFactory.Create)),
      NavigationBookmarks => new(GetBookmarkedNotes().Select(_noteViewModelFactory.Create)),
      NavigationTrash => new(GetTrashedNotes().Select(_noteViewModelFactory.Create)),
      _ => new(),
    };
    _currentNoteViewModels = noteViewModels;
    return noteViewModels;
  }
}
