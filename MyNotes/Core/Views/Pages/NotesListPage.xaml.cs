using MyNotes.Core.Models;

namespace MyNotes.Core.Views;
public sealed partial class NotesListPage : Page
{
  public NotesListPage()
  {
    this.InitializeComponent();
  }

  public NavigationBoardItem? Navigation { get; private set; }
  protected override void OnNavigatedTo(NavigationEventArgs e)
  {
    base.OnNavigatedTo(e);
    Navigation = (NavigationBoardItem)e.Parameter;
  }
}
