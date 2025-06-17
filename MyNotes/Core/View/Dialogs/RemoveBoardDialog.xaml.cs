using MyNotes.Core.Model;

namespace MyNotes.Core.View;
internal sealed partial class RemoveBoardDialog : ContentDialog
{
  public RemoveBoardDialog(NavigationBoard board)
  {
    InitializeComponent();
    Board = board;
    Title = $"Delete {Board.Name}?";
  }

  public NavigationBoard Board { get; }
}
