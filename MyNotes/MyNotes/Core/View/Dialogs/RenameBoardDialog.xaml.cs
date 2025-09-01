using MyNotes.Core.Service;

namespace MyNotes.Core.View;
internal sealed partial class RenameBoardDialog : ContentDialog
{
  public RenameBoardDialog(DialogService.NavigationDialogViewModel viewModel)
  {
    InitializeComponent();
    ViewModel = viewModel;
    this.Unloaded += RenameBoardDialog_Unloaded;
  }

  private void RenameBoardDialog_Unloaded(object sender, RoutedEventArgs e) => this.Bindings.StopTracking();

  public DialogService.NavigationDialogViewModel ViewModel { get; }
}
