using MyNotes.Core.Models;

namespace MyNotes.Core.Views;
public sealed partial class ListsGroupPage : Page
{
  public ListsGroupPage()
  {
    this.InitializeComponent();
  }

  public NavigationItem? Navigation { get; private set; }
  protected override void OnNavigatedTo(NavigationEventArgs e)
  {
    base.OnNavigatedTo(e);
    Navigation = (NavigationBoardGroupItem)e.Parameter;
  }
}
