namespace MyNotes.Core.Views;
public sealed partial class TestPage : Page
{
  public TestPage()
  {
    this.InitializeComponent();
  }

  protected override void OnNavigatedTo(NavigationEventArgs e)
  {
    base.OnNavigatedTo(e);
    VIEW_TitleTextBlock.Text = (e.Parameter as string) ?? "NULL";
  }
}
