using MyNotes.Core.ViewModel;

namespace MyNotes.Core.View;
internal sealed partial class NoteInformationDialog : ContentDialog
{
  public NoteInformationDialog(NoteViewModel viewModel)
  {
    InitializeComponent();
    ViewModel = viewModel;
  }

  public NoteViewModel ViewModel { get; }
}
