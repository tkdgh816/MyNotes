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
    Navigation.PropertyChanged -= OnNavigationPropertyChanged;
    ViewModel.Dispose();
  }

  public NoteBoardViewModel ViewModel { get; }

  public NavigationBoardGroupItem Navigation { get; private set; } = null!;
  protected override void OnNavigatedTo(NavigationEventArgs e)
  {
    base.OnNavigatedTo(e);
    Navigation = (NavigationBoardGroupItem)e.Parameter;
    Navigation.PropertyChanged += OnNavigationPropertyChanged;
  }
  private void OnNavigationPropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (e.PropertyName == "Icon")
      ViewModel.DatabaseService.UpdateBoard(Navigation, "Icon");
  }
}
