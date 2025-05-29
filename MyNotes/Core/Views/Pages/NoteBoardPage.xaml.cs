using MyNotes.Core.Models;
using MyNotes.Core.ViewModels;

namespace MyNotes.Core.Views;

public sealed partial class NoteBoardPage : Page
{
  public NoteBoardPage()
  {
    InitializeComponent();
    ViewModel = App.Current.GetService<NoteBoardViewModel>();

    this.Unloaded += NoteBoardPage_Unloaded;
  }

  public NoteBoardViewModel ViewModel { get; }

  public NavigationNotes Navigation { get; private set; } = null!;

  protected override void OnNavigatedTo(NavigationEventArgs e)
  {
    base.OnNavigatedTo(e);
    Navigation = (NavigationNotes)e.Parameter;
    RegisterEvents();
  }

  private void NoteBoardPage_Unloaded(object sender, RoutedEventArgs e)
  {
    UnregisterEvents();
    ViewModel.Dispose();
  }


  #region Handling Events
  private void RegisterEvents()
  {
    _viewStyleSliderDebounceTimer.Tick += OnViewStyleSliderTimerTick;
  }

  private void UnregisterEvents()
  {
    _viewStyleSliderDebounceTimer.Tick -= OnViewStyleSliderTimerTick;
  }
  #endregion

  #region Timers
  readonly DispatcherTimer _viewStyleSliderDebounceTimer = new() { Interval = TimeSpan.FromMilliseconds(400) };

  private void OnViewStyleSliderTimerTick(object? sender, object e)
    => ChangeViewSize();
  #endregion

  #region VisualStates
  private void Template_RootUserControl_PointerEntered(object sender, PointerRoutedEventArgs e)
    => VisualStateManager.GoToState((Control)sender, "HoverStateHovering", false);

  private void Template_RootUserControl_PointerExited(object sender, PointerRoutedEventArgs e)
    => VisualStateManager.GoToState((Control)sender, "HoverStateNormal", false);
  #endregion

  #region Change View Style
  bool _isViewGridStyle = true;
  private void View_StyleChangeRadioButton_Click(object sender, RoutedEventArgs e)
  {
    switch ((string)((RadioButton)sender).Tag)
    {
      case "GridStyle":
        _isViewGridStyle = true;
        View_NotesGridView.ItemsPanel = (ItemsPanelTemplate)App.Current.Resources["AppItemPanelTemplate_GridStyle"];
        View_NotesGridView.ItemTemplate = (DataTemplate)this.Resources["GridViewItemTemplate_GridStyle"];
        break;
      case "ListStyle":
        _isViewGridStyle = false;
        View_NotesGridView.ItemsPanel = (ItemsPanelTemplate)App.Current.Resources["AppItemPanelTemplate_ListStyle"];
        View_NotesGridView.ItemTemplate = (DataTemplate)this.Resources["GridViewItemTemplate_ListStyle"];
        break;
    }
    ChangeViewSize();
    View_NotesGridView.UpdateLayout();
  }
  
  double _styleSliderValue = 210;
  private void View_StyleSizeChangeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
  {
    if (View_NotesGridView is null)
      return;
    _viewStyleSliderDebounceTimer.Stop();
    _styleSliderValue = e.NewValue;
    _viewStyleSliderDebounceTimer.Start();
  }

  double _ratioSliderValue = 50;
  private void View_StyleRatioChangeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
  {
    if (View_NotesGridView is null)
      return;
    _viewStyleSliderDebounceTimer.Stop();
    _ratioSliderValue = e.NewValue;
    _viewStyleSliderDebounceTimer.Start();
  }

  private void ChangeViewSize()
  {
    string styleName = "AppGridViewItemContainerStyle" + (_styleSliderValue, _ratioSliderValue, _isViewGridStyle) switch
    {
      (150, 0, true) => "150Short",
      (180, 0, true) => "180Short",
      (210, 0, true) => "210Short",
      (240, 0, true) => "240Short",
      (270, 0, true) => "270Short",
      (150, 50, true) => "150Moderate",
      (180, 50, true) => "180Moderate",
      (210, 50, true) => "210Moderate",
      (240, 50, true) => "240Moderate",
      (270, 50, true) => "270Moderate",
      (150, 100, true) => "150Tall",
      (180, 100, true) => "180Tall",
      (210, 100, true) => "210Tall",
      (240, 100, true) => "240Tall",
      (270, 100, true) => "270Tall",
      (150, _, false) => "60",
      (180, _, false) => "75",
      (210, _, false) => "90",
      (240, _, false) => "105",
      (270, _, false) => "120",
      _ => "210Moderate"
    };

    View_NotesGridView.ItemContainerStyle = (Style)((App)Application.Current).Resources[styleName];
    View_NotesGridView.UpdateLayout();
  }
  #endregion
}
