using MyNotes.Core.Models;
using MyNotes.Core.Services;

namespace MyNotes.Core.ViewModels;

public class NoteBoardViewModel : ViewModelBase
{
  public NoteBoardViewModel(WindowService windowService)
  {
    WindowService = windowService;
  }

  public WindowService WindowService { get; } = null!;

  public void CreateNoteWindow(Note note)
  {
    WindowService.CreateNoteWindow(note).Activate();
  }
}
