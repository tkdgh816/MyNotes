namespace MyNotes.Core.View;

internal sealed partial class RemoveNoteDialog : ContentDialog
{
  public RemoveNoteDialog(int noteCount)
  {
    InitializeComponent();
    this.Content = $"Remove {noteCount} note(s)";
  }
}
