using MyNotes.Common.Commands;
using MyNotes.Common.Messaging;
using MyNotes.Core.Dto;
using MyNotes.Core.Model;
using MyNotes.Core.Shared;

namespace MyNotes.Core.ViewModel;
internal partial class NoteViewModel : ViewModelBase
{
  public Command<bool>? OpenWindowCommand { get; private set; }
  public Command? MinimizeWindowCommand { get; private set; }
  public Command? CloseWindowCommand { get; private set; }
  public Command? ToggleWindowPinCommand { get; private set; }
  public Command<int>? SetWindowBackdropCommand { get; private set; }
  public Command? BookmarkNoteCommand { get; private set; }
  public Command? RemoveNoteCommand { get; private set; }
  public Command? RestoreNoteCommand { get; private set; }
  public Command? DeleteNoteCommand { get; private set; }
  public Command? ShowMoveNoteToBoardDialogCommand { get; private set; }
  public Command<BoardId>? MoveNoteToBoardCommand { get; private set; }
  public Command? ShowRenameNoteTitleDialogCommand { get; private set; }
  public Command<string>? RenameNoteTitleCommand { get; private set; }
  public Command? ShowEditNoteTagsDialogCommmand { get; private set; }
  public Command? ShowNoteInformationDialogOnMainCommand { get; private set; }
  public Command<XamlRoot>? ShowNoteInformationDialogCommand { get; private set; }
  public Command<TagCommandParameterDto>? AddNoteTagCommand { get; private set; }
  public Command<Tag>? DeleteNoteTagCommand { get; private set; }

  private void SetCommands()
  {
    OpenWindowCommand = new((enabled) =>
    {
      if (enabled)
        _windowService.GetNoteWindow(Note).Activate();
    });

    MinimizeWindowCommand = new(() => _windowService.MinimizeNoteWindow(Note));
    CloseWindowCommand = new(() => _windowService.CloseNoteWindow(Note));
    ToggleWindowPinCommand = new(() => _windowService.ToggleNoteWindowPin(Note, IsWindowAlwaysOnTop = !IsWindowAlwaysOnTop));
    SetWindowBackdropCommand = new((backdropKindIndex) => _windowService.SetNoteWindowBackdrop(Note, (BackdropKind)backdropKindIndex));

    BookmarkNoteCommand = new(() =>
    {
      Note.IsBookmarked = !Note.IsBookmarked;
      if (!Note.IsBookmarked)
        WeakReferenceMessenger.Default.Send(new Message<NoteViewModel>(this), Tokens.RemoveNoteFromBookmarks);
    });

    RemoveNoteCommand = new(() =>
    {
      Note.IsTrashed = true;
      WeakReferenceMessenger.Default.Send(new Message<NoteViewModel>(this), Tokens.RemoveNoteFromBoard);
    });

    RestoreNoteCommand = new(() =>
    {
      Note.IsTrashed = false;
      WeakReferenceMessenger.Default.Send(new Message<NoteViewModel>(this), Tokens.RemoveNoteFromBoard);
    });

    // TODO: Delete와 Move시 Note와 NoteViewModel 명시적 Dispose해야 함
    DeleteNoteCommand = new(() =>
    {
      _noteService.DeleteNote(Note);
      WeakReferenceMessenger.Default.Send(new Message<NoteViewModel>(this), Tokens.RemoveNoteFromBoard);
    });

    ShowMoveNoteToBoardDialogCommand = new(async () =>
    {
      var (dialogResult, boardId) = await _dialogService.ShowMoveNoteToBoardDialog();
      if (dialogResult && boardId is not null)
        MoveNoteToBoardCommand?.Execute(boardId);
    });

    MoveNoteToBoardCommand = new(async (boardId) =>
    {
      if (boardId != Note.BoardId)
      {
        Note.BoardId = boardId;
        await _noteService.UpdateNote(Note, NoteUpdateFields.Parent);
        WeakReferenceMessenger.Default.Send(new Message<NoteViewModel>(this), Tokens.RemoveNoteFromBoard);
      }
    });

    ShowRenameNoteTitleDialogCommand = new(() => _dialogService.ShowRenameNoteTitleDialog(this));
    RenameNoteTitleCommand = new((title) =>
    {
      title = title.Trim();
      if (!string.IsNullOrWhiteSpace(title))
        Note.Title = title;
    });

    ShowEditNoteTagsDialogCommmand = new(() => _dialogService.ShowEditNoteTagsDialog(this));
    ShowNoteInformationDialogOnMainCommand = new(() => _dialogService.ShowNoteInformationDialog(this));
    ShowNoteInformationDialogCommand = new((xamlRoot) => _dialogService.ShowNoteInformationDialog(this, xamlRoot));

    AddNoteTagCommand = new((dto) =>
    {
      if (!string.IsNullOrEmpty(dto.Text))
      {
        if (!Note.Tags.Any(tag => tag.Text == dto.Text))
        {
          Tag newTag = _tagService.CreateTag(dto.Text, dto.Color);
          Note.Tags.Add(newTag);
        }
      }
    });

    DeleteNoteTagCommand = new((tag) =>
    {
      if (_tagService.DeleteTagFromNote(Note, tag))
        Note.Tags.Remove(tag);
    });
  }
}
