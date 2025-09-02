using MyNotes.Core.ViewModel;

namespace MyNotes.Core.View;
internal sealed partial class RenameNoteTitleDialog : ContentDialog
{
  public RenameNoteTitleDialog(NoteViewModel noteViewModel)
  {
    InitializeComponent();
    ViewModel = noteViewModel;

    this.Unloaded += RenameNoteTitleDialog_Unloaded;
  }

  private void RenameNoteTitleDialog_Unloaded(object sender, RoutedEventArgs e) => this.Bindings.StopTracking();

  public NoteViewModel ViewModel { get; }
}
