using MyNotes.Core.ViewModels;

namespace MyNotes.Core.Views;
public sealed partial class NoteWindow : Window
{
  public NoteWindow()
  {
    this.InitializeComponent();
    this.ExtendsContentIntoTitleBar = true;

    double _dpi = 1.25;
    OverlappedPresenter presenter = (OverlappedPresenter)AppWindow.Presenter;
    presenter.SetBorderAndTitleBar(true, false);
    presenter.PreferredMinimumWidth = (int)(400 * _dpi);
    presenter.PreferredMinimumHeight = (int)(400 * _dpi);

    AppWindow.Resize(new((int)(400 * _dpi), (int)(400 * _dpi)));


    ViewModel = App.Current.GetService<NoteViewModel>();
  }

  public NoteViewModel ViewModel { get; }

  private void VIEW_CloseButton_Click(object sender, RoutedEventArgs e)
  {
    ViewModel.CloseWindow();
  }
}
