using MyNotes.Core.Model;

namespace MyNotes.Core.View;
internal sealed partial class DeleteBoardDialog : ContentDialog
{
  public DeleteBoardDialog(NavigationBoard board)
  {
    InitializeComponent();
    Board = board;
  }

  public NavigationBoard Board { get; }
}
