using MyNotes.Core.Model;

namespace MyNotes.Core.View;
internal sealed partial class DeleteBoardDialog : ContentDialog
{
  public DeleteBoardDialog(NavigationBoard board)
  {
    InitializeComponent();
    Board = board;
    this.Unloaded += DeleteBoardDialog_Unloaded;
  }

  private void DeleteBoardDialog_Unloaded(object sender, RoutedEventArgs e) => this.Bindings.StopTracking();

  public NavigationBoard Board { get; }
}
