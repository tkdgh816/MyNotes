using MyNotes.Core.Model;

namespace MyNotes.Core.View;

internal sealed partial class SearchPage : Page
{
  public SearchPage()
  {
    this.InitializeComponent();
  }

  protected override void OnNavigatedTo(NavigationEventArgs e)
  {
    base.OnNavigatedTo(e);
    View_TitleTextBlock.Text = "Search results for " + ((NavigationItem)e.Parameter).Name;
  }
}
