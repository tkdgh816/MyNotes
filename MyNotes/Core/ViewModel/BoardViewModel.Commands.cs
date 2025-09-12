using CommunityToolkit.WinUI;

using Microsoft.UI.Dispatching;

using MyNotes.Common.Collections;
using MyNotes.Common.Commands;
using MyNotes.Core.Model;
using MyNotes.Core.Shared;
using MyNotes.Debugging;

namespace MyNotes.Core.ViewModel;

internal partial class BoardViewModel : ViewModelBase
{
  public Command? AddNewNoteCommand { get; private set; }
  public Command<IList<object>>? RemoveNotesCommand { get; private set; }
  public Command<IList<object>>? ShowMoveNoteToBoardDialogCommand { get; private set; }
  public Command<string>? ChangeSortFieldCommand { get; private set; }
  public Command<string>? ChangeSortDirectionCommand { get; private set; }
  public Command? ShowRenameBoardDialogCommand { get; private set; }
  public Command? ShowDeleteBoardDialogCommand { get; private set; }
  public Command<Icon>? ChangeIconCommand { get; private set; }
  public Command<string>? Debug_AddNewNoteCommand { get; private set; }

  private void SetCommands()
  {
    AddNewNoteCommand = new(() =>
    {
      Note newNote = _noteService.CreateNote((NavigationUserBoard)_navigation);
      NoteViewModel noteViewModel = _noteViewModelFactory.Resolve(newNote);

      Notes.Add(newNote);
      int index = Notes.IndexOf(newNote);
      if (index <= NoteViewModels.Count)
        NoteViewModels.Insert(index, noteViewModel);

      noteViewModel.CreateWindow();
    });

    Debug_AddNewNoteCommand = new((language) =>
    {
      void Debug_CreateNote(int count, string language)
      {
        for (int i = 0; i < count; i++)
        {
          Note newNote = _noteService.CreateNote((NavigationUserBoard)_navigation);
          newNote.Title = TextGenerator.GenerateTitle(language);
          newNote.Body = TextGenerator.GenerateTexts(1000, language);
          NoteViewModel noteViewModel = _noteViewModelFactory.Resolve(newNote);

          Notes.Add(newNote);
          int index = Notes.IndexOf(newNote);
          if (index <= NoteViewModels.Count)
            NoteViewModels.Insert(index, noteViewModel);

          noteViewModel.CreateWindow();

          DispatcherQueue.GetForCurrentThread().EnqueueAsync(async () =>
          {
            await Task.Delay(5000);
            noteViewModel.CloseWindowCommand?.Execute();
          });
        }
      }
      Debug_CreateNote(25, language);
    });

    RemoveNotesCommand = new(async (items) =>
    {
      var noteViewModels = items.OfType<NoteViewModel>().ToList();
      int count = noteViewModels.Count;
      if (count == 0)
        return;

      if (await _dialogService.ShowRemoveNoteDialog(count))
      {
        foreach (var noteViewModel in noteViewModels)
        {
          Notes.Remove(noteViewModel.Note);
          noteViewModel.RemoveNoteCommand?.Execute();
        }
      }
    });

    ShowMoveNoteToBoardDialogCommand = new(async (items) =>
    {
      var noteViewModels = items.OfType<NoteViewModel>().ToList();
      int count = noteViewModels.Count;
      if (count == 0)
        return;

      var (dialogResult, boardId) = await _dialogService.ShowMoveNoteToBoardDialog();
      if (dialogResult && boardId is not null)
      {
        if (_navigation is not NavigationUserBoard userBoard || userBoard.Id != boardId)
          foreach (var noteViewModel in noteViewModels)
          {
            Notes.Remove(noteViewModel.Note);
            noteViewModel.MoveNoteToBoardCommand?.Execute(boardId);
          }
      }
    });

    ChangeSortFieldCommand = new((key) =>
    {
      if (Enum.TryParse<NoteSortField>(key, out var sortField))
      {
        SortField = sortField;
        SortNoteViewModels();
        _settingsService.SetBoardSettings(AppSettingsKeys.BoardNoteSortField, (int)sortField);
      }
    });
    ChangeSortDirectionCommand = new((direction) =>
    {
      if (Enum.TryParse<SortDirection>(direction, out var sortDirection))
      {
        SortDirection = sortDirection;
        SortNoteViewModels();
        _settingsService.SetBoardSettings(AppSettingsKeys.BoardNoteSortDirection, (int)sortDirection);
      }
    });

    ShowRenameBoardDialogCommand = new(async () =>
    {
      if (_navigation is NavigationUserBoard userBoard)
      {
        var result = await _dialogService.ShowRenameBoardDialog(userBoard);
        if (result.DialogResult)
        {
          if (result.Icon is not null)
            _navigation.Icon = result.Icon;
          if (result.Name is not null)
            _navigation.Name = result.Name;
        }
      }
    });

    ShowDeleteBoardDialogCommand = new(async () =>
    {
      if (_navigation is NavigationUserBoard userBoard)
        await _dialogService.ShowDeleteBoardDialog(userBoard);
    });

    ChangeIconCommand = new((icon) => _navigation.Icon = icon);
  }
}