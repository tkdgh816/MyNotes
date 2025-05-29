using CommunityToolkit.Mvvm.Messaging;

using MyNotes.Common.Messaging;
using MyNotes.Core.Models;
using MyNotes.Core.ViewModels;

namespace MyNotes.Core.Views;
public sealed partial class NoteHubPage : Page
{
  public NoteHubPage()
  {
    this.InitializeComponent();
    ViewModel = App.Current.GetService<NoteHubViewModel>();
    MainViewModel = App.Current.GetService<MainViewModel>();
    TagsViewModel = App.Current.GetService<TagsViewModel>();
    this.Unloaded += NoteBoardPage_Unloaded;
    _inputTimer.Tick += OnInputTimerTick;
    RegisterMessenger();
  }

  private void NoteBoardPage_Unloaded(object sender, RoutedEventArgs e)
  {
    Navigation.PropertyChanged -= OnNavigationPropertyChanged;
    _viewStyleSliderTimer.Tick -= OnTimerTick;
    ViewModel.Dispose();
  }

  public NoteHubViewModel ViewModel { get; }
  public MainViewModel MainViewModel { get; }
  public TagsViewModel TagsViewModel { get; }

  public NavigationNotes Navigation { get; private set; } = null!;
  protected override void OnNavigatedTo(NavigationEventArgs e)
  {
    base.OnNavigatedTo(e);
    Navigation = (NavigationNotes)e.Parameter;
    Navigation.PropertyChanged += OnNavigationPropertyChanged;
    _viewStyleSliderTimer.Tick += OnTimerTick;
  }
  private void OnNavigationPropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (e.PropertyName == "Icon")
      ViewModel.DatabaseService.UpdateBoard((NavigationBoard)Navigation, "Icon");
  }

  private void GridViewItemTemplateRoot_PointerEntered(object sender, PointerRoutedEventArgs e)
    => VisualStateManager.GoToState((Control)sender, "HoverStateHovering", false);

  private void GridViewItemTemplateRoot_PointerExited(object sender, PointerRoutedEventArgs e)
    => VisualStateManager.GoToState((Control)sender, "HoverStateNormal", false);

  private void GridView_ChoosingItemContainer(ListViewBase sender, ChoosingItemContainerEventArgs args)
  {
    //Debug.WriteLine($"Choosing {args.ItemIndex} : {args.Item.GetType()} {args.ItemContainer?.GetType()}");
  }

  private void GridView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
  {
    //Debug.WriteLine($"Changing {args.ItemIndex} : {args.Item.GetType()} {args.ItemContainer?.GetType()}");
  }

  bool isItemsWrapGridStyle = true;
  private void View_StyleChangeRadioButton_Click(object sender, RoutedEventArgs e)
  {
    if (sender is RadioButton item)
    {
      switch ((string)item.Tag)
      {
        case "GridStyle":
          isItemsWrapGridStyle = true;
          View_NotesGridView.ItemsPanel = (ItemsPanelTemplate)((App)Application.Current).Resources["AppItemPanelTemplate1"];
          View_NotesGridView.ItemTemplate = (DataTemplate)this.Resources["GridViewItemTemplate1"];
          break;
        case "ListStyle":
          isItemsWrapGridStyle = false;
          View_NotesGridView.ItemsPanel = (ItemsPanelTemplate)((App)Application.Current).Resources["AppItemPanelTemplate2"];
          View_NotesGridView.ItemTemplate = (DataTemplate)this.Resources["GridViewItemTemplate2"];
          break;
      }
      ChangeViewSize();
      View_NotesGridView.UpdateLayout();
    }
  }

  readonly DispatcherTimer _viewStyleSliderTimer = new() { Interval = TimeSpan.FromMilliseconds(400) };
  double _styleSliderValue = 240;
  private void View_StyleChangeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
  {
    if (View_NotesGridView is null)
      return;
    _viewStyleSliderTimer.Stop();
    _styleSliderValue = e.NewValue;
    _viewStyleSliderTimer.Start();
  }

  double _ratioSliderValue = 50;
  private void View_RatioChangeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
  {
    if (View_NotesGridView is null)
      return;
    _viewStyleSliderTimer.Stop();
    _ratioSliderValue = e.NewValue;
    _viewStyleSliderTimer.Start();
  }

  private void ChangeViewSize()
  {
    string styleName = "AppGridViewItemContainerStyle" + (_styleSliderValue, _ratioSliderValue, isItemsWrapGridStyle) switch
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
  private void OnTimerTick(object? sender, object e)
  {
    ChangeViewSize();
  }

  private void View_SearchAutoSuggestBox_LostFocus(object sender, RoutedEventArgs e)
  {
    VisualStateManager.GoToState(this, "SearchBoxNormal", false);
  }

  private void View_SearchButton_Click(object sender, RoutedEventArgs e)
  {
    VisualStateManager.GoToState(this, "SearchBoxSearching", false);
  }

  private void View_SearchAutoSuggestBox_LayoutUpdated(object sender, object e)
  {
    if (View_SearchAutoSuggestBox.Visibility is Visibility.Visible)
      View_SearchAutoSuggestBox.Focus(FocusState.Programmatic);
  }

  private void AppTagButton_Click(object sender, RoutedEventArgs e)
  {
    Note note = (Note)((FrameworkElement)sender).DataContext;
    Debug.WriteLine(note.Title);
  }

  private async void View_EditTagsButton_Click(object sender, RoutedEventArgs e)
  {
    //NoteViewModel noteViewModel = (NoteViewModel)((FrameworkElement)sender).DataContext;
    //View_EditTagsContentDialog.DataContext = noteViewModel;
    //View_EditTagsItemsRepeater.ItemsSource = noteViewModel.Note.Tags;
    //View_EditTagsHeaderTextBlock.Text = noteViewModel.Note.Title;
    //await View_EditTagsContentDialog.ShowAsync();
  }

  private void View_AddTagButton_Click(object sender, RoutedEventArgs e)
  {
    NoteViewModel noteViewModel = (NoteViewModel)((FrameworkElement)sender).DataContext;
    AddTag(noteViewModel);
  }

  private void AddTag(NoteViewModel noteViewModel)
  {
    //string tag = View_AddTagAutoSuggestBox.Text.Trim();
    //TagsViewModel.AddTag(tag);
    //noteViewModel.UpdateNoteTag(tag);
    //View_AddTagAutoSuggestBox.Text = "";
  }

  private void View_AddTagAutoSuggestBox_GotFocus(object sender, RoutedEventArgs e)
  {
    ViewTagsSuggestion();
  }

  private void View_AddTagAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
  {
    if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
      ViewTagsSuggestion();
  }

  DispatcherTimer _inputTimer = new() { Interval = TimeSpan.FromMilliseconds(300) };

  private void OnInputTimerTick(object? sender, object e)
  {
    //string inputText = View_AddTagAutoSuggestBox.Text;
    //View_AddTagAutoSuggestBox.ItemsSource = string.IsNullOrEmpty(inputText)
    //  ? TagsViewModel.TagGroup.GetTagsAll()
    //  : TagsViewModel.TagGroup.FindTags(inputText);
    //_inputTimer.Stop();
  }

  private void ViewTagsSuggestion()
  {
    _inputTimer.Stop();
    _inputTimer.Start();
  }

  private void View_AddTagAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
  {
    NoteViewModel noteViewModel = (NoteViewModel)((FrameworkElement)sender).DataContext;
    AddTag(noteViewModel);
  }

  private void RegisterMessenger()
  {
    WeakReferenceMessenger.Default.Register<DialogRequestMessage, string>(this, Tokens.MoveNoteToBoard, new(async (recipient, message) =>
    {
      NoteViewModel noteViewModel = (NoteViewModel)message.Content!;
      if (ViewModel.NoteViewModels.Contains(noteViewModel))
      {
        //if (await View_MoveToBoardContentDialog.ShowAsync() == ContentDialogResult.Primary)
        //{
        //  NavigationBoard? selected = View_MoveToBoardTreeView.SelectedItem as NavigationBoard;
        //  if (selected is not null)
        //  {
        //    ViewModel.NoteViewModels.Remove(noteViewModel);
        //    noteViewModel.Note.BoardId = selected.Id;
        //    noteViewModel.UpdateNote(nameof(Note.BoardId));
        //  }
        //}
      }
    }));
  }
}