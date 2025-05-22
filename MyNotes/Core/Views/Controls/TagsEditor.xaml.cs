using CommunityToolkit.WinUI;

using MyNotes.Core.Models;

namespace MyNotes.Core.Views;

public sealed partial class TagsEditor : UserControl
{
  public TagsEditor()
  {
    this.InitializeComponent();

    TagsCollectionViewSource.Source = TagGroup;
  }

  public CollectionViewSource TagsCollectionViewSource { get; } = new() { IsSourceGrouped = true };

  public TagGroup TagGroup { get; } = new()
  {
    new ("M", ["Mango", "Melon", "Mouse", ]),
    new ("A", ["Apple", "Apricot", "Ant", ]),
    new ("O", ["Owl", ]),
    new ("P", ["Peach", "Parrot", ]),
    new ("B", ["Banana", "Blackberry", "Blueberry","Bear", ]),
    new ("C", ["Cherry", "Cat", "Camel", "Cow", "Chicken", ]),
    new ("D", ["Dog", "Donkey", "Dolphin", ]),
    new ("N", ["Navy", ]),
    new ("R", ["Rabbit", ]),
    new ("E", ["Elephant", "Eagle",]),
    new ("F", ["Frog", ]),
    new ("S", ["Swan", "Snake",]),
    new ("W", ["Whale", ]),
    new ("G", ["Grapefruit", "Grapes", "Goat", ]),
    new ("H", ["Horse", ]),
    new ("K", ["Kiwi", ]),
    new ("L", ["Lizard", ]),
  };

  private void VIEW_SearchAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
  {
    string queryText = args.QueryText;
    if (string.IsNullOrWhiteSpace(queryText))
    {
      TagsCollectionViewSource.Source = TagGroup;
    }
    else
    {
      TagsCollectionViewSource.Source =
        from grp in TagGroup
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
    TagsCollectionViewSource.Source = TagGroup;
    VisualStateManager.GoToState(this, "EditorModeAdd", false);
  }

  private void VIEW_DeleteButtonMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    VIEW_DeleteTagsTeachingTip.IsOpen = true;
  }

  private void VIEW_CloseAddModeButton_Click(object sender, RoutedEventArgs e)
  {
    VisualStateManager.GoToState(this, "EditorModeNormal", false);
  }

  private void VIEW_AddTagButton_Click(object sender, RoutedEventArgs e)
  {
    string tag = VIEW_TagInputTextBox.Text;
    if (!string.IsNullOrWhiteSpace(tag))
    {
      TagGroup.AddItem(tag);
    }
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
      TagGroup.RemoveItem(tag);
    VIEW_DeleteTagsTeachingTip.IsOpen = false;
  }

  private void VIEW_CommandBarExploreButton_Click(object sender, RoutedEventArgs e)
  {

  }
}
