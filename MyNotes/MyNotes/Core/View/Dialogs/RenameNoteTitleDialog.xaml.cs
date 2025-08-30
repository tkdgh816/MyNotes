using MyNotes.Core.ViewModel;

namespace MyNotes.Core.View;
internal sealed partial class RenameNoteTitleDialog : ContentDialog
{
  public RenameNoteTitleDialog(NoteViewModel noteViewModel)
  {
    InitializeComponent();
    ViewModel = noteViewModel;
  }

  public NoteViewModel ViewModel { get; }
}
