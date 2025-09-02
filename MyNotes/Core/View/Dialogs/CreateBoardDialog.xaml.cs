using MyNotes.Core.Service;

namespace MyNotes.Core.View;
internal sealed partial class CreateBoardDialog : ContentDialog
{
  public CreateBoardDialog(DialogService.NavigationDialogViewModel viewModel)
  {
    InitializeComponent();
    ViewModel = viewModel;

    this.Unloaded += CreateBoardDialog_Unloaded;
  }

  private void CreateBoardDialog_Unloaded(object sender, RoutedEventArgs e) => this.Bindings.StopTracking();

  public DialogService.NavigationDialogViewModel ViewModel { get; }
}
