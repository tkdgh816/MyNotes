using MyNotes.Core.Service;

namespace MyNotes.Core.View;
internal sealed partial class CreateBoardDialog : ContentDialog
{
  public CreateBoardDialog(DialogService.NavigationDialogViewModel viewModel)
  {
    InitializeComponent();
    ViewModel = viewModel;
  }

  public DialogService.NavigationDialogViewModel ViewModel { get; }
}
