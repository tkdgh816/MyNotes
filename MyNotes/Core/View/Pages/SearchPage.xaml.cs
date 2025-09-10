using System.Text;

using CommunityToolkit.WinUI;

using Microsoft.UI.Xaml.Documents;

using MyNotes.Core.Model;
using MyNotes.Core.Shared;
using MyNotes.Core.ViewModel;

using Windows.ApplicationModel.DataTransfer;

using AppColorHelper = MyNotes.Common.Helpers.ColorHelper;
using ToolkitColorHelper = CommunityToolkit.WinUI.Helpers.ColorHelper;


namespace MyNotes.Core.View;

internal sealed partial class SearchPage : Page
{
  public SearchPage()
  {
    InitializeComponent();

    this.Unloaded += SearchPage_Unloaded;
  }

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

  private void SearchPage_Unloaded(object sender, RoutedEventArgs e)
  {
    UnregisterEvents();
    ViewModel.Dispose();

    this.Bindings.StopTracking();
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

  private void View_NotesGridView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
  {
    var noteViewModel = (NoteViewModel)args.Item;
    var container = (GridViewItem)args.ItemContainer;
    var templateRoot = (UserControl)container.ContentTemplateRoot;

    if (args.InRecycleQueue)
    {
      container.GotFocus -= OnHoverStateHovering;
      container.LostFocus -= OnHoverStateNormal;
    }
    else
    {
      container.GotFocus += OnHoverStateHovering;
      container.LostFocus += OnHoverStateNormal;

      if (templateRoot.FindChild("Template_RootGrid") is Grid grid
        && grid.FindChild("Template_BodyTextBlock") is TextBlock textblock)
      {
        textblock.TextHighlighters.Clear();
        var background = noteViewModel.Note.Background;
        var comp = AppColorHelper.GetComplementary(background);

        var backHsv = ToolkitColorHelper.ToHsv(background);
        var h = (backHsv.H - 180.0) / 360.0;

        var emphasis = ToolkitColorHelper.FromHsv((h - Math.Floor(h)) * 360.0, Math.Clamp(backHsv.S, 0.3, 0.7), Math.Max(0.7, backHsv.V), 1.0);

        foreach (var textRange in noteViewModel.Note.HighlighterRanges)
        {
          TextHighlighter highlighter = new() { Background = new SolidColorBrush(emphasis) };
          highlighter.Ranges.Add(textRange);
          textblock.TextHighlighters.Add(highlighter);
        }
      }
    }
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
      ViewModel.SetBoardSettings(AppSettingsKeys.BoardViewStyle, viewStyle);

    ViewModel.RefreshSearchPreview();
  }
  #endregion

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

  private void View_SelectAllAppBarButton_Click(object sender, RoutedEventArgs e) => View_NotesGridView.SelectAll();
  private void View_DeselectAllAppBarButton_Click(object sender, RoutedEventArgs e) => View_NotesGridView.DeselectAll();

  private void View_NotesGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    if (_isEditMode)
    {

    }
  }

  private void View_NotesGridView_ChoosingItemContainer(ListViewBase sender, ChoosingItemContainerEventArgs args)
  {
    //Debug.WriteLine(args.ItemIndex);
    //Debug.WriteLine(args.ItemContainer);
  }
}