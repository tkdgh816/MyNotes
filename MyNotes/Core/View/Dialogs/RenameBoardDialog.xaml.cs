using MyNotes.Core.Service;

namespace MyNotes.Core.View;
internal sealed partial class RenameBoardDialog : ContentDialog
{
  public RenameBoardDialog(DialogService.NavigationDialogViewModel viewModel)
  {
    InitializeComponent();
    ViewModel = viewModel;
  }

  public DialogService.NavigationDialogViewModel ViewModel { get; }
}
