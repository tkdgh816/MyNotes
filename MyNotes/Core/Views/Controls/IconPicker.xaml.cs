using MyNotes.Core.Models;

namespace MyNotes.Core.Views;

public sealed partial class IconPicker : UserControl
{
  public IconPicker()
  {
    this.InitializeComponent();
  }

  public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(Icon), typeof(IconPicker), new PropertyMetadata(null));
  public Icon Icon
  {
    get => (Icon)GetValue(IconProperty);
    set => SetValue(IconProperty, value);
  }

  private void FontButton_Click(object sender, RoutedEventArgs e)
  {
    Icon = (Glyph)((FrameworkElement)sender).DataContext;
  }

  private void EmojiButton_Click(object sender, RoutedEventArgs e)
  {
    Icon = (Emoji)((FrameworkElement)sender).DataContext;
  }

  private void View_PrimarySelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
  {
    string selectedTag = (string)sender.SelectedItem.Tag;
    switch (selectedTag)
    {
      case "Basic":
        ControlSecondarySelectorBar(false);
        View_IconsItemsRepeater.ItemsSource = IconLibrary.Glyphs;
        break;
      case "PeopleAndBody":
        ControlSecondarySelectorBar(true);
        break;
      default:
        ControlSecondarySelectorBar(false);
        View_IconsItemsRepeater.ItemsSource = IconLibrary.EmojisList[selectedTag];
        break;
    }
    View_IconsViewScrollView.ScrollTo(0, 0);
  }

  private void ControlSecondarySelectorBar(bool isActivated)
  {
    if(isActivated)
    {
      View_SecondarySelectorBar.Visibility = Visibility.Visible;
      View_SecondarySelectorBar.SelectedItem = View_PeopleAndBodyGeneralSelectorBarItem;
    }
    else
    {
      View_SecondarySelectorBar.Visibility = Visibility.Collapsed;
      View_SecondarySelectorBar.SelectedItem = null;
    }
  }

  private void View_SecondarySelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
  {
    if (sender.SelectedItem is null)
      return;

    string selectedTag = (string)sender.SelectedItem.Tag;
    View_IconsItemsRepeater.ItemsSource = IconLibrary.EmojisList[selectedTag];
  }
}

public class IconPickerViewItemTemplateSelector : DataTemplateSelector
{
  public DataTemplate? FontItemTemplate { get; set; }
  public DataTemplate? EmojiItemTemplate { get; set; }

  protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
  {
    return item switch
    {
      Glyph => FontItemTemplate,
      Emoji => EmojiItemTemplate,
      _ => null
    };
  }
}