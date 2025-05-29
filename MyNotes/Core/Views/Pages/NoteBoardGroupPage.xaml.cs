using MyNotes.Core.Models;
using MyNotes.Core.ViewModels;

namespace MyNotes.Core.Views;
public sealed partial class NoteBoardGroupPage : Page
{
  public NoteBoardGroupPage()
  {
    this.InitializeComponent();
    ViewModel = App.Current.GetService<NoteBoardViewModel>();
    this.Unloaded += NoteBoardGroupPage_Unloaded;
  }

  private void NoteBoardGroupPage_Unloaded(object sender, RoutedEventArgs e)
  {
    ViewModel.Dispose();
  }

  public NoteBoardViewModel ViewModel { get; }

  public NavigationBoardGroup Navigation { get; private set; } = null!;
  protected override void OnNavigatedTo(NavigationEventArgs e)
  {
    base.OnNavigatedTo(e);
    Navigation = (NavigationBoardGroup)e.Parameter;
  }
}
