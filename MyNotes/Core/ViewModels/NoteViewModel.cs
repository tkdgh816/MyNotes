using MyNotes.Core.Models;
using MyNotes.Core.Services;

namespace MyNotes.Core.ViewModels;

public class NoteViewModel : ViewModelBase
{
  public NoteViewModel(WindowService windowService)
  {
    WindowService = windowService;
    Note = WindowService.CurrentNote!;
  }

  public WindowService WindowService { get; }
  public Note Note { get; }

  public bool CloseWindow() => WindowService.CloseNoteWindow(Note);
}
