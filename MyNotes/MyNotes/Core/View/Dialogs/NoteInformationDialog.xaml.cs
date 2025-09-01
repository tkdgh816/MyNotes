using MyNotes.Core.ViewModel;

namespace MyNotes.Core.View;
internal sealed partial class NoteInformationDialog : ContentDialog
{
  public NoteInformationDialog(NoteViewModel viewModel)
  {
    InitializeComponent();
    ViewModel = viewModel;
    this.Unloaded += NoteInformationDialog_Unloaded;
  }

  private void NoteInformationDialog_Unloaded(object sender, RoutedEventArgs e) => this.Bindings.StopTracking();

  public NoteViewModel ViewModel { get; }
}
