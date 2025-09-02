using MyNotes.Core.Model;
using MyNotes.Core.ViewModel;

using static MyNotes.Common.Interop.NativeMethods;

namespace MyNotes.Core.View;

internal sealed partial class NoteWindow : Window
{
  public NoteWindow(Note note)
  {
    this.InitializeComponent();
    this.ExtendsContentIntoTitleBar = true;
    View_NotePage.ViewModel = App.Instance.GetService<NoteViewModelFactory>().Resolve(note);
  }
}
