using CommunityToolkit.WinUI;

using MyNotes.Core.Model;
using MyNotes.Core.ViewModel;
using MyNotes.Debugging;

namespace MyNotes.Core.View;

internal sealed partial class TagsEditor : UserControl
{
  public TagsEditor()
  {
    this.InitializeComponent();
    ViewModel = App.Instance.GetService<TagsViewModel>();
    ReferenceTracker.ViewReferences.Add(new(this.GetType().Name, this));
  }

  public TagsViewModel ViewModel { get; }

  private void View_SelectedTagButton_Click(object sender, RoutedEventArgs e)
  {
    Tag tag = (Tag)((Button)sender).DataContext;
    View_TagsListView.MakeVisible(new SemanticZoomLocation() { Item = tag, Bounds = new Rect(0.0, 50.0, 0.0, 0.0) });
  }

  private void View_SelectedTagButton_DeleteButtonClick(object sender, RoutedEventArgs e)
  {
    Tag tag = (Tag)((Button)sender).DataContext;
    View_TagsListView.SelectedItems.Remove(tag);
  }

  private void View_CommandBarDeselectAllButton_Click(object sender, RoutedEventArgs e) 
    => View_TagsListView.DeselectAll();

  private void View_TagsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    foreach (Tag tag in e.AddedItems.Cast<Tag>())
      ViewModel.SelectedTags.Add(tag);
    foreach (Tag tag in e.RemovedItems.Cast<Tag>())
      ViewModel.SelectedTags.Remove(tag);
  }
}
