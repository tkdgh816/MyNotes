using CommunityToolkit.WinUI;

using MyNotes.Core.Models;
using MyNotes.Core.ViewModels;

namespace MyNotes.Core.Views;

public sealed partial class TagsEditor : UserControl
{
  public TagsEditor()
  {
    this.InitializeComponent();
    ViewModel = ((App)Application.Current).GetService<TagsViewModel>();
  }

  public TagsViewModel ViewModel { get; }
  private void View_SearchAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
  {
    string queryText = args.QueryText;
    if (string.IsNullOrWhiteSpace(queryText))
    {
      ViewModel.TagsCollectionViewSource.Source = ViewModel.TagGroup;
    }
    else
    {
      ViewModel.TagsCollectionViewSource.Source =
        from grp in ViewModel.TagGroup
        select new Tags(grp.Key, grp.Where(tag => tag.Contains(queryText, StringComparison.CurrentCultureIgnoreCase)));
    }
  }

  private void View_SelectedTagButton_Click(object sender, RoutedEventArgs e)
  {
    string tag = (string)((Button)sender).Content;
    View_TagsListView.MakeVisible(new SemanticZoomLocation() { Item = tag, Bounds = new Rect(0.0, 50.0, 0.0, 0.0) });
  }

  private void View_SelectedTagButton_DeleteButtonClick(object sender, RoutedEventArgs e)
  {
    string tag = (string)((Button)sender).Content;
    View_TagsListView.SelectedItems.Remove(tag);
  }

  private void View_AddButtonMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    ViewModel.TagsCollectionViewSource.Source = ViewModel.TagGroup;
    VisualStateManager.GoToState(this, "EditorModeAdd", false);
  }

  private void View_DeleteButtonMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    if (View_TagsListView.SelectedItems.Count > 0)
      View_DeleteTagsTeachingTip.IsOpen = true;
  }

  private void View_CloseAddModeButton_Click(object sender, RoutedEventArgs e)
  {
    VisualStateManager.GoToState(this, "EditorModeNormal", false);
  }

  private void View_AddTagButton_Click(object sender, RoutedEventArgs e)
  {
    ViewModel.AddTag(View_TagInputTextBox.Text);
  }

  private void View_TagsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    if (View_TagsListView.SelectedItems.Count > 0)
      VisualStateManager.GoToState(this, "Selected", false);
    else
      VisualStateManager.GoToState(this, "Unselected", false);
  }

  private void View_CommandBarDeselectAllButton_Click(object sender, RoutedEventArgs e)
  {
    View_TagsListView.DeselectAll();
  }

  private void View_DeleteTagsTeachingTip_ActionButtonClick(TeachingTip sender, object args)
  {
    foreach (string tag in View_TagsListView.SelectedItems.Cast<string>().ToArray())
      ViewModel.DeleteTag(tag);
    View_DeleteTagsTeachingTip.IsOpen = false;
  }

  private void View_CommandBarExploreButton_Click(object sender, RoutedEventArgs e)
  {

  }

  private void View_CommandBarInclusionModeButton_Click(object sender, RoutedEventArgs e)
  {
    ViewModel.IsIntersectSelection = !ViewModel.IsIntersectSelection;
  }
}
