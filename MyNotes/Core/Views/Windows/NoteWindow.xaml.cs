using MyNotes.Core.Models;
using MyNotes.Core.ViewModels;

namespace MyNotes.Core.Views;

public sealed partial class NoteWindow : Window
{
  public NoteWindow(Note note)
  {
    this.InitializeComponent();
    this.ExtendsContentIntoTitleBar = true;
    View_NotePage.ViewModel = App.Current.GetService<NoteViewModelFactory>().Create(note);
    AppWindow.MoveAndResize(new RectInt32(_X: note.Position.X, _Y: note.Position.Y, _Width: (int)(note.Size.Width * _dpi), _Height: (int)(note.Size.Height * _dpi)));
  }

  static double _dpi = 1.25;
}
