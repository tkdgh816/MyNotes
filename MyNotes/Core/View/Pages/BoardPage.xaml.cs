using System.Text;

using CommunityToolkit.WinUI;

using MyNotes.Core.Model;
using MyNotes.Core.Shared;
using MyNotes.Core.ViewModel;

using Windows.ApplicationModel.DataTransfer;

namespace MyNotes.Core.View;

internal sealed partial class BoardPage : Page
{
  public BoardPage()
  {
    InitializeComponent();

    this.Unloaded += BoardPage_Unloaded;
  }

  private void BoardPage_Unloaded(object sender, RoutedEventArgs e) => this.Bindings.StopTracking();

  public NavigationBoard Navigation { get; private set; } = null!;
  public BoardViewModel ViewModel { get; private set; } = null!;

  protected override void OnNavigatedTo(NavigationEventArgs e)
  {
    base.OnNavigatedTo(e);
    Navigation = (NavigationBoard)e.Parameter;
    ViewModel = App.Instance.GetService<BoardViewModelFactory>().Resolve(Navigation);

    InitializeSettings();
    RegisterEvents();
  }

  protected override void OnNavigatedFrom(NavigationEventArgs e)
  {
    UnregisterEvents();
    this.Bindings.StopTracking();
    ViewModel.Dispose();
    base.OnNavigatedFrom(e);
  }

  private void InitializeSettings()
  {
    foreach (var item in View_SortMenuFlyoutSubItem.Items.OfType<RadioMenuFlyoutItem>())
    {
      if (item.GroupName == "SortField")
        item.IsChecked = item.Text == ViewModel.SortField.ToString();
      else if (item.GroupName == "SortDirection")
        item.IsChecked = item.Text == ViewModel.SortDirection.ToString();
    }

    var styleName = ViewModel.ViewStyle.ToString().Split('_');

    View_StyleChangeRadioButtons.SelectedIndex = (styleName[0] == "Grid") ? 0 : 1;

    if (double.TryParse(styleName[1], out double size))
      _sizeSliderValue = size;

    if (styleName.Length == 3)
      _ratioSliderValue = double.TryParse(styleName[2], out double ratio) ? ratio : 50;

    ChangeViewStyle(styleName[0]);
    ChangeViewSize();
  }
  private void NoteViewModels_MoreItemsLoaded(object? sender, Common.Collections.MoreItemsLoadedEventArgs e)
  {
    if (_isEditMode)
    {
      if (ViewModel.IsChecked == true)
        View_NotesGridView.SelectAll();
    }
  }

  #region Handling Events
  private void RegisterEvents()
  {
    _viewStyleSliderDebounceTimer.Tick += OnViewStyleSliderTimerTick;
    //ViewModel.NoteViewModels.MoreItemsLoaded += NoteViewModels_MoreItemsLoaded;
  }

  private void UnregisterEvents()
  {
    _viewStyleSliderDebounceTimer.Tick -= OnViewStyleSliderTimerTick;
    //ViewModel.NoteViewModels.MoreItemsLoaded -= NoteViewModels_MoreItemsLoaded;
  }
  #endregion

  #region Timers
  private readonly DispatcherTimer _viewStyleSliderDebounceTimer = new() { Interval = TimeSpan.FromMilliseconds(500) };
  private void OnViewStyleSliderTimerTick(object? sender, object e) => ChangeViewSize();
  #endregion

  #region Change UI and VisualStates
  private void Template_RootUserControl_PointerEntered(object sender, PointerRoutedEventArgs e)
  {
    if (!_isEditMode)
      VisualStateManager.GoToState((Control)sender, "HoverStateHovering", false);
  }

  private void Template_RootUserControl_PointerExited(object sender, PointerRoutedEventArgs e)
  {
    if (!_isEditMode)
      VisualStateManager.GoToState((Control)sender, "HoverStateNormal", false);
  }

  private void OnHoverStateHovering(object sender, RoutedEventArgs args)
  {
    if (sender is GridViewItem container && container.ContentTemplateRoot is Control rootControl)
      VisualStateManager.GoToState(rootControl, "HoverStateHovering", false);
    Debug.WriteLine("Hovering");
  }
  private void OnHoverStateNormal(object sender, RoutedEventArgs args)
  {
    if (sender is GridViewItem container && container.ContentTemplateRoot is Control rootControl)
      VisualStateManager.GoToState(rootControl, "HoverStateNormal", false);
  }
  #endregion

  #region Change View Style
  private void View_StyleChangeRadioButton_Click(object sender, RoutedEventArgs e)
  {
    ChangeViewStyle((string)((RadioButton)sender).Tag);
    ChangeViewSize();
  }

  private double _sizeSliderValue;
  public double SizeSliderValue
  {
    get => _sizeSliderValue;
    set
    {
      if (_sizeSliderValue == value)
        return;

      _viewStyleSliderDebounceTimer.Stop();
      _sizeSliderValue = value;
      _viewStyleSliderDebounceTimer.Start();
    }
  }

  private double _ratioSliderValue;
  public double RatioSliderValue
  {
    get => _ratioSliderValue;
    set
    {
      if (_ratioSliderValue == value)
        return;

      _viewStyleSliderDebounceTimer.Stop();
      _ratioSliderValue = value;
      _viewStyleSliderDebounceTimer.Start();
    }
  }

  private void ChangeViewStyle(string styleName)
  {
    if (styleName == "Grid")
    {
      View_NotesGridView.ItemsPanel = (ItemsPanelTemplate)App.Instance.Resources["AppItemPanelTemplate_GridStyle"];
      View_NotesGridView.ItemTemplate = (DataTemplate)this.Resources["GridViewItemTemplate_GridStyle"];
    }
    else if (styleName == "List")
    {
      View_NotesGridView.ItemsPanel = (ItemsPanelTemplate)App.Instance.Resources["AppItemPanelTemplate_ListStyle"];
      View_NotesGridView.ItemTemplate = (DataTemplate)this.Resources["GridViewItemTemplate_ListStyle"];
    }
  }

  private void ChangeViewSize()
  {
    string styleNameSuffix = (View_StyleChangeRadioButtons.SelectedIndex <= 0) ? $"Grid_{SizeSliderValue}_{RatioSliderValue}" : $"List_{SizeSliderValue}";

    View_NotesGridView.ItemContainerStyle = (Style)((App)Application.Current).Resources[$"AppGridViewItemContainerStyle_{styleNameSuffix}"];

    if (Enum.TryParse<BoardViewStyle>(styleNameSuffix, out var viewStyle))
      ViewModel.SetBoardSettings(AppSettingsKeys.BoardViewStyle, (int)viewStyle);
  }
  #endregion

  private void View_IconPicker_IconChanged(IconPicker sender, IconChangedEventArgs args)
    => ViewModel.ChangeIconCommand?.Execute(args.NewIcon);

  private void View_NotesGridView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
  {
    var notes = e.Items.Cast<NoteViewModel>().Select(vm => vm.Note);
    if (!notes.Any())
      return;

    StringBuilder text = new();
    foreach (var note in notes)
    {
      text.AppendLine($"[{note.Title}]");
      text.AppendLine(note.Preview);
      text.AppendLine();
    }

    e.Data.SetData(StandardDataFormats.Text, text.ToString());
    e.Data.SetData(DataFormats.Note, string.Join(':', notes.Select(note => note.Id.ToString())));
  }

  private bool _isEditMode;
  private void View_EditNotesMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    _isEditMode = true;
    View_HeaderGrid.Translation = new(0, 0, 16);
    VisualStateManager.GoToState(this, "EditModeEdit", false);
  }

  private void View_RevertAppBarButton_Click(object sender, RoutedEventArgs e)
  {
    _isEditMode = false;
    View_HeaderGrid.Translation = new(0, 0, 0);
    VisualStateManager.GoToState(this, "EditModeNormal", false);
  }

  private void View_NotesGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    if (_isEditMode)
    {
      int selectedCount = View_NotesGridView.SelectedItems.Count;
      View_SelectAllAppBarToggleButton.IsChecked = selectedCount > 0 && selectedCount == ViewModel.NoteViewModels.Count;
    }
  }

  private void View_NotesGridView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
  {
    var noteViewModel = (NoteViewModel)args.Item;
    var container = (GridViewItem)args.ItemContainer;

    if (args.InRecycleQueue)
    {
      container.GotFocus -= OnHoverStateHovering;
      container.LostFocus -= OnHoverStateNormal;
    }
    else
    {
      container.GotFocus += OnHoverStateHovering;
      container.LostFocus += OnHoverStateNormal;
    }
  }

  private void View_NotesGridView_ChoosingItemContainer(ListViewBase sender, ChoosingItemContainerEventArgs args)
  {

  }

  private void View_SelectAllAppBarToggleButton_Click(object sender, RoutedEventArgs e)
  {
    if (ViewModel.IsChecked == true)
      View_NotesGridView.SelectAll();
    else
      View_NotesGridView.DeselectAll();
  }
}