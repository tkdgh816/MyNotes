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
  private void VIEW_SearchAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
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

  private void VIEW_SelectedTagButton_Click(object sender, RoutedEventArgs e)
  {
    string tag = (string)((Button)sender).Content;
    VIEW_TagsListView.MakeVisible(new SemanticZoomLocation() { Item = tag, Bounds = new Rect(0.0, 50.0, 0.0, 0.0) });
  }

  private void VIEW_SelectedTagButton_DeleteButtonClick(object sender, RoutedEventArgs e)
  {
    string tag = (string)((Button)sender).Content;
    VIEW_TagsListView.SelectedItems.Remove(tag);
  }

  private void VIEW_AddButtonMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    ViewModel.TagsCollectionViewSource.Source = ViewModel.TagGroup;
    VisualStateManager.GoToState(this, "EditorModeAdd", false);
  }

  private void VIEW_DeleteButtonMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    if (VIEW_TagsListView.SelectedItems.Count > 0)
      VIEW_DeleteTagsTeachingTip.IsOpen = true;
  }

  private void VIEW_CloseAddModeButton_Click(object sender, RoutedEventArgs e)
  {
    VisualStateManager.GoToState(this, "EditorModeNormal", false);
  }

  private void VIEW_AddTagButton_Click(object sender, RoutedEventArgs e)
  {
    ViewModel.AddTag(VIEW_TagInputTextBox.Text);
  }

  private void VIEW_TagsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    if (VIEW_TagsListView.SelectedItems.Count > 0)
      VisualStateManager.GoToState(this, "Selected", false);
    else
      VisualStateManager.GoToState(this, "Unselected", false);
  }

  private void VIEW_CommandBarDeselectAllButton_Click(object sender, RoutedEventArgs e)
  {
    VIEW_TagsListView.DeselectAll();
  }

  private void VIEW_DeleteTagsTeachingTip_ActionButtonClick(TeachingTip sender, object args)
  {
    foreach (string tag in VIEW_TagsListView.SelectedItems.Cast<string>().ToArray())
      ViewModel.DeleteTag(tag);
    VIEW_DeleteTagsTeachingTip.IsOpen = false;
  }

  private void VIEW_CommandBarExploreButton_Click(object sender, RoutedEventArgs e)
  {

  }

  private void VIEW_CommandBarInclusionModeButton_Click(object sender, RoutedEventArgs e)
  {
    ViewModel.IsIntersectSelection = !ViewModel.IsIntersectSelection;
  }
}
