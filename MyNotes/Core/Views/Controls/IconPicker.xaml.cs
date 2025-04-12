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

  private void SymbolButton_Click(object sender, RoutedEventArgs e)
  {
    Icon = new SymbolIconSource() { Symbol = (Symbol)((FrameworkElement)sender).DataContext };
  }

  private void EmojiButton_Click(object sender, RoutedEventArgs e)
  {
    Icon = new BitmapIconSource() { UriSource = new Uri((string)((FrameworkElement)sender).DataContext), ShowAsMonochrome = false };
  }

  private void VIEW_PrimarySelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
  {
    string selectedTag = (string)sender.SelectedItem.Tag;
    switch (selectedTag)
    {
      case "Symbol":
        VIEW_SecondarySelectorBar.Visibility = Visibility.Collapsed;
        VIEW_IconsItemsRepeater.ItemsSource = Models.Icon.SymbolPaths;
        break;
      case "PeopleAndBody":
        VIEW_SecondarySelectorBar.Visibility = Visibility.Visible;
        VIEW_SecondarySelectorBar.SelectedItem = VIEW_PeopleAndBodyGeneralSelectorBarItem;
        break;
      default:
        VIEW_SecondarySelectorBar.Visibility = Visibility.Collapsed;
        Models.Icon.EmojiPaths.TryGetValue(selectedTag, out var items);
        VIEW_IconsItemsRepeater.ItemsSource = items;
        break;
    }
    VIEW_IconsViewScrollViewer.ChangeView(0, 0, 1, true);
  }

  private void VIEW_SecondarySelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
  {
    string selectedTag = (string)sender.SelectedItem.Tag;
    Models.Icon.EmojiPaths.TryGetValue(selectedTag, out var items);
    VIEW_IconsItemsRepeater.ItemsSource = items;
  }
}

public class IconPickerViewItemTemplateSelector : DataTemplateSelector
{
  public DataTemplate? SymbolItemTemplate { get; set; }
  public DataTemplate? EmojiItemTemplate { get; set; }

  protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
  {
    return item switch
    {
      Symbol => SymbolItemTemplate,
      string => EmojiItemTemplate,
      _ => null
    };
  }
}