using MyNotes.Core.Model;

namespace MyNotes.Core.View;

internal sealed partial class IconPicker : Button
{
  public IconPicker()
  {
    this.InitializeComponent();
  }

  public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(Icon), typeof(IconPicker), new PropertyMetadata(null, OnIconChanged));
  public Icon Icon
  {
    get => (Icon)GetValue(IconProperty);
    set => SetValue(IconProperty, value);
  }

  private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
  {
    IconPicker iconPicker = (IconPicker)d;
    iconPicker.IconChanged?.Invoke(iconPicker, new IconChangedEventArgs((Icon)args.OldValue, (Icon)args.NewValue));
  }

  private void FontButton_Click(object sender, RoutedEventArgs e)
    => Icon = (Glyph)((FrameworkElement)sender).DataContext;

  private void EmojiButton_Click(object sender, RoutedEventArgs e)
    => Icon = (Emoji)((FrameworkElement)sender).DataContext;

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
    if (isActivated)
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

  public event TypedEventHandler<IconPicker, IconChangedEventArgs>? IconChanged;
}

internal class IconPickerViewItemTemplateSelector : DataTemplateSelector
{
  public DataTemplate? FontItemTemplate { get; set; }
  public DataTemplate? EmojiItemTemplate { get; set; }

  protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container) => item switch
  {
    Glyph => FontItemTemplate,
    Emoji => EmojiItemTemplate,
    _ => null
  };
}

internal class IconChangedEventArgs(Icon oldIcon, Icon newIcon) : EventArgs
{
  public Icon? OldIcon { get; } = oldIcon;
  public Icon NewIcon { get; } = newIcon;
}