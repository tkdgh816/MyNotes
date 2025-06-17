using MyNotes.Core.ViewModel;

namespace MyNotes.Core.View;

internal sealed partial class EditNoteTagsDialog : ContentDialog
{
  public EditNoteTagsDialog(NoteViewModel noteViewModel)
  {
    InitializeComponent();
    ViewModel = noteViewModel;
  }

  public NoteViewModel ViewModel { get; }
}
