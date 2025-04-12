using MyNotes.Core.Models;

namespace MyNotes.Core.Views;

public sealed partial class SearchPage : Page
{
  public SearchPage()
  {
    this.InitializeComponent();
  }

  protected override void OnNavigatedTo(NavigationEventArgs e)
  {
    base.OnNavigatedTo(e);
    VIEW_TitleTextBlock.Text = "Search results for " + ((NavigationItem)e.Parameter).Name;
  }
}
