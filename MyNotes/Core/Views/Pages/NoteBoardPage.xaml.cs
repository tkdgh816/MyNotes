using MyNotes.Core.Models;
using MyNotes.Core.ViewModels;

namespace MyNotes.Core.Views;
public sealed partial class NoteBoard : Page
{
  public NoteBoard()
  {
    this.InitializeComponent();
    ViewModel = App.Current.GetService<NoteBoardViewModel>();
  }

  public NoteBoardViewModel ViewModel { get; }

  public NavigationBoardItem? Navigation { get; private set; }
  protected override void OnNavigatedTo(NavigationEventArgs e)
  {
    base.OnNavigatedTo(e);
    Navigation = (NavigationBoardItem)e.Parameter;
  }

  private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
  {
    VisualStateManager.GoToState((Control)sender, "HoverStateHovering", false);
  }

  private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
  {
    VisualStateManager.GoToState((Control)sender, "HoverStateNormal", false);
  }

  private void GridView_ChoosingItemContainer(ListViewBase sender, ChoosingItemContainerEventArgs args)
  {
    //Debug.WriteLine($"Choosing {args.ItemIndex} : {args.Item.GetType()} {args.ItemContainer?.GetType()}");
  }

  private void GridView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
  {
    //Debug.WriteLine($"Changing {args.ItemIndex} : {args.Item.GetType()} {args.ItemContainer?.GetType()}");
  }

  bool isItemsWrapGridStyle = true;
  private void VIEW_StyleChangeRadioButton_Click(object sender, RoutedEventArgs e)
  {
    if (sender is RadioButton item)
    {
      switch ((string)item.Tag)
      {
        case "Style1":
          isItemsWrapGridStyle = true;
          VIEW_NotesGridView.ItemsPanel = (ItemsPanelTemplate)((App)Application.Current).Resources["AppItemPanelTemplate1"];
          VIEW_NotesGridView.ItemContainerStyle = (Style)((App)Application.Current).Resources["AppGridViewItemContainerStyle210"];
          VIEW_NotesGridView.ItemTemplate = (DataTemplate)this.Resources["GridViewItemTemplate1"];
          break;
        case "Style2":
          isItemsWrapGridStyle = false;
          VIEW_NotesGridView.ItemsPanel = (ItemsPanelTemplate)((App)Application.Current).Resources["AppItemPanelTemplate2"];
          VIEW_NotesGridView.ItemContainerStyle = (Style)((App)Application.Current).Resources["AppGridViewItemContainerStyle90"];
          VIEW_NotesGridView.ItemTemplate = (DataTemplate)this.Resources["GridViewItemTemplate2"];
          break;
      }
      VIEW_StyleChangeSlider.Value = 210;
      VIEW_NotesGridView.UpdateLayout();
    }
  }

  private void VIEW_StyleChangeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
  {
    if (VIEW_NotesGridView is null)
      return;

    switch (e.NewValue)
    {
      case 150:
        VIEW_NotesGridView.ItemContainerStyle = isItemsWrapGridStyle ? (Style)((App)Application.Current).Resources["AppGridViewItemContainerStyle150"] : (Style)((App)Application.Current).Resources["AppGridViewItemContainerStyle60"];
        break;
      case 180:
        VIEW_NotesGridView.ItemContainerStyle = isItemsWrapGridStyle ? (Style)((App)Application.Current).Resources["AppGridViewItemContainerStyle180"] : (Style)((App)Application.Current).Resources["AppGridViewItemContainerStyle75"];
        break;
      case 210:
        VIEW_NotesGridView.ItemContainerStyle = isItemsWrapGridStyle ? (Style)((App)Application.Current).Resources["AppGridViewItemContainerStyle210"] : (Style)((App)Application.Current).Resources["AppGridViewItemContainerStyle90"];
        break;
      case 240:
        VIEW_NotesGridView.ItemContainerStyle = isItemsWrapGridStyle ? (Style)((App)Application.Current).Resources["AppGridViewItemContainerStyle240"] : (Style)((App)Application.Current).Resources["AppGridViewItemContainerStyle105"];
        break;
      case 270:
        VIEW_NotesGridView.ItemContainerStyle = isItemsWrapGridStyle ? (Style)((App)Application.Current).Resources["AppGridViewItemContainerStyle270"] : (Style)((App)Application.Current).Resources["AppGridViewItemContainerStyle120"];
        break;
    }
    VIEW_NotesGridView.UpdateLayout();
  }

  int num = 0;
  private void Button_Click(object sender, RoutedEventArgs e)
  {
    Debug.WriteLine("Button Clicked " + num++);
  }

  private void VIEW_SearchAutoSuggestBox_LostFocus(object sender, RoutedEventArgs e)
  {
    VisualStateManager.GoToState(this, "SearchBoxNormal", false);
  }

  private void VIEW_SearchButton_Click(object sender, RoutedEventArgs e)
  {
    VisualStateManager.GoToState(this, "SearchBoxSearching", false);
  }

  private void VIEW_SearchAutoSuggestBox_LayoutUpdated(object sender, object e)
  {
    if (VIEW_SearchAutoSuggestBox.Visibility is Visibility.Visible)
      VIEW_SearchAutoSuggestBox.Focus(FocusState.Programmatic);
  }

  private void UserControl_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
  {
    ViewModel.CreateNoteWindow((Note)((FrameworkElement)sender).DataContext);
  }
}
