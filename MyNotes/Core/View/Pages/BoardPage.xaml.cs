using MyNotes.Common.Messaging;
using MyNotes.Core.Model;
using MyNotes.Core.Service;
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

    _settingsService = App.Instance.GetService<SettingsService>();
    InitializeSettings();
  }

  public NavigationBoard Navigation { get; private set; } = null!;
  public BoardViewModel ViewModel { get; private set; } = null!;
  private readonly SettingsService _settingsService;

  protected override void OnNavigatedTo(NavigationEventArgs e)
  {
    base.OnNavigatedTo(e);
    Navigation = (NavigationBoard)e.Parameter;
    ViewModel = App.Instance.GetService<BoardViewModelFactory>().Resolve(Navigation);
    //this.DataContext = ViewModel;

    RegisterEvents();
    RegisterMessengers();
  }

  private void BoardPage_Unloaded(object sender, RoutedEventArgs e)
  {
    UnregisterEvents();
    UnregisterMessengers();
    ViewModel.Dispose();
  }

  private void InitializeSettings()
  {
    var settings = _settingsService.GetBoardSettings();
    foreach (var item in View_SortMenuFlyoutSubItem.Items.OfType<RadioMenuFlyoutItem>())
    {
      if (item.GroupName == "SortField")
        item.IsChecked = item.Text == settings.SortField.ToString();
      else if (item.GroupName == "SortDirection")
        item.IsChecked = item.Text == settings.SortDirection.ToString();
    }

    var styleName = settings.ViewStyle.ToString().Split('_');

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
    => VisualStateManager.GoToState((Control)sender, "HoverStateHovering", false);

  private void Template_RootUserControl_PointerExited(object sender, PointerRoutedEventArgs e)
    => VisualStateManager.GoToState((Control)sender, "HoverStateNormal", false);

  private void View_SearchButton_Click(object sender, RoutedEventArgs e)
  => VisualStateManager.GoToState(this, "SearchBoxSearching", false);

  private void View_SearchAutoSuggestBox_LostFocus(object sender, RoutedEventArgs e)
    => VisualStateManager.GoToState(this, "SearchBoxNormal", false);

  private void View_SearchAutoSuggestBox_LayoutUpdated(object sender, object e)
  {
    if (View_SearchAutoSuggestBox.Visibility is Visibility.Visible)
      View_SearchAutoSuggestBox.Focus(FocusState.Programmatic);
  }

  private void View_NotesGridView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
  {
    if (args.InRecycleQueue)
      return;

    if (args.ItemContainer is GridViewItem container)
    {
      if (container.ContentTemplateRoot is Control rootControl)
      {
        container.GotFocus += (s, e) => VisualStateManager.GoToState(rootControl, "HoverStateHovering", false);
        container.LostFocus += (s, e) => VisualStateManager.GoToState(rootControl, "HoverStateNormal", false);
      }
    }
  }
  #endregion

  #region Change View Style
  private void View_StyleChangeRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    ChangeViewStyle(View_StyleChangeRadioButtons.SelectedIndex <= 0 ? "Grid" : "List");
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
      _settingsService.SetBoardSettings(AppSettingsKeys.BoardViewStyle, (int)viewStyle);
  }
  #endregion

  private void View_IconPicker_IconChanged(IconPicker sender, IconChangedEventArgs args)
    => ViewModel.ChangeIconCommand?.Execute(args.NewIcon);

  #region Messengers
  private void RegisterMessengers()
  {
    WeakReferenceMessenger.Default.Register<Message<IEnumerable<NoteViewModel>>, string>(this, Tokens.ChangeSourceFiltered, new((recipient, message) =>
    {
      if (message.Sender == ViewModel)
      {
        NotesCollectionViewSource.Source = message.Content;
        VisualStateManager.GoToState(this, "CannotAddNote", false);
      }
    }));

    WeakReferenceMessenger.Default.Register<Message<IEnumerable<NoteViewModel>>, string>(this, Tokens.ChangeSourceUnfiltered, new((recipient, message) =>
    {
      if (message.Sender == ViewModel)
      {
        NotesCollectionViewSource.Source = message.Content;
        VisualStateManager.GoToState(this, "CanAddNote", false);
      }
    }));
  }

  private void UnregisterMessengers()
  {
    WeakReferenceMessenger.Default.UnregisterAll(this);
  }
  #endregion

  private void View_NotesGridView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
  {
    NoteViewModel noteViewModel = (NoteViewModel)e.Items[0];

    NoteId noteId = noteViewModel.Note.Id;
    string noteTitle = noteViewModel.Note.Title;
    string noteBody = noteViewModel.Note.Preview;
    var container = (FrameworkElement)View_NotesGridView.ContainerFromItem(noteViewModel);

    e.Data.SetData(StandardDataFormats.Text, $"[{noteTitle}]\r\n{noteBody}");
    e.Data.SetData(DataFormats.Note, noteId.Value.ToString());
  }

  private void View_RemoveNotesMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    View_NotesGridView.SelectionMode = ListViewSelectionMode.Multiple;
  }
}
