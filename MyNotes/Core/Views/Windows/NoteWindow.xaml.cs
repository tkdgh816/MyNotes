using MyNotes.Core.Models;
using MyNotes.Core.Services;

namespace MyNotes.Core.Views;

public sealed partial class NoteWindow : Window
{
  public NoteWindow()
  {
    this.InitializeComponent();
    this.ExtendsContentIntoTitleBar = true;
    WindowService windowService = ((App)Application.Current).GetService<WindowService>();
    Note note = windowService.CurrentNote!;
    AppWindow.MoveAndResize(new RectInt32(_X: note.Position.X, _Y: note.Position.Y, _Width: (int)(note.Size.Width * _dpi), _Height: (int)(note.Size.Height * _dpi)));
  }

  static double _dpi = 1.25;
}
