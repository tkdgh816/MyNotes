using MyNotes.Core.Model;
using MyNotes.Core.ViewModel;

namespace MyNotes.Core.View;
internal sealed partial class UserGroupPage : Page
{
  public UserGroupPage()
  {
    this.InitializeComponent();
    this.Unloaded += NoteBoardGroupPage_Unloaded;
  }

  private void NoteBoardGroupPage_Unloaded(object sender, RoutedEventArgs e)
  {
    ViewModel.Dispose();

    this.Bindings.StopTracking();
  }

  public BoardViewModel ViewModel { get; private set; } = null!;

  public NavigationUserGroup Navigation { get; private set; } = null!;
  protected override void OnNavigatedTo(NavigationEventArgs e)
  {
    base.OnNavigatedTo(e);
    Navigation = (NavigationUserGroup)e.Parameter;
    ViewModel = App.Instance.GetService<BoardViewModelFactory>().Resolve(Navigation);
  }
}
