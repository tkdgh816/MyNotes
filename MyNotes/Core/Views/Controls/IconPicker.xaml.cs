using MyNotes.Core.Models;

namespace MyNotes.Core.Views;

public sealed partial class IconPicker : UserControl
{
  public IconPicker()
  {
    this.InitializeComponent();
  }

  public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(IconSource), typeof(IconPicker), new PropertyMetadata(null));
  public IconSource Icon
  {
    get => (IconSource)GetValue(IconProperty);
    set => SetValue(IconProperty, value);
  }

  private void FontButton_Click(object sender, RoutedEventArgs e)
  {
    Icon = new FontIconSource() { Glyph = ((Glyph)((FrameworkElement)sender).DataContext).Code };
  }

  private void EmojiButton_Click(object sender, RoutedEventArgs e)
  {
    Icon = new BitmapIconSource() { UriSource = new Uri(((Emoji)((FrameworkElement)sender).DataContext).Path), ShowAsMonochrome = false };
  }

  private void VIEW_PrimarySelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
  {
    string selectedTag = (string)sender.SelectedItem.Tag;
    switch (selectedTag)
    {
      case "Basic":
        ControlSecondarySelectorBar(false);
        VIEW_IconsItemsRepeater.ItemsSource = Models.Icon.Glyphs;
        break;
      case "PeopleAndBody":
        ControlSecondarySelectorBar(true);
        break;
      default:
        ControlSecondarySelectorBar(false);
        Models.Icon.Emojis.TryGetValue(selectedTag, out var items);
        VIEW_IconsItemsRepeater.ItemsSource = items;
        break;
    }
    VIEW_IconsViewScrollViewer.ChangeView(0, 0, 1, true);
  }

  private void ControlSecondarySelectorBar(bool isActivated)
  {
    if(isActivated)
    {
      VIEW_SecondarySelectorBar.Visibility = Visibility.Visible;
      VIEW_SecondarySelectorBar.SelectedItem = VIEW_PeopleAndBodyGeneralSelectorBarItem;
    }
    else
    {
      VIEW_SecondarySelectorBar.Visibility = Visibility.Collapsed;
      VIEW_SecondarySelectorBar.SelectedItem = null;
    }
  }

  private void VIEW_SecondarySelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
  {
    if (sender.SelectedItem is null)
      return;

    string selectedTag = (string)sender.SelectedItem.Tag;
    Models.Icon.Emojis.TryGetValue(selectedTag, out var items);
    VIEW_IconsItemsRepeater.ItemsSource = items;
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