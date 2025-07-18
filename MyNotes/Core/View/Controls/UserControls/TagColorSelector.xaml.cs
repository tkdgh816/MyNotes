using MyNotes.Core.Model;

namespace MyNotes.Core.View;

internal sealed partial class TagColorSelector : Button
{
  public TagColorSelector()
  {
    InitializeComponent();
  }

  public static readonly DependencyProperty TagColorProperty = DependencyProperty.Register("TagColor", typeof(TagColor), typeof(TagColorSelector), new PropertyMetadata(TagColor.Transparent, OnTagColorChanged));
  public TagColor TagColor
  {
    get => (TagColor)GetValue(TagColorProperty);
    set => SetValue(TagColorProperty, value);
  }

  private static void OnTagColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
  {
    TagColorSelector tagColorSelector = (TagColorSelector)d;
    tagColorSelector.TagColorChanged?.Invoke(tagColorSelector, new TagColorChangedEventArgs((TagColor)args.OldValue, (TagColor)args.NewValue));
  }

  public List<TagColor> Colors { get; } = [.. Enum.GetValues<TagColor>()];

  private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    => TagColor = ((GridView)sender).SelectedItem is TagColor tagColor ? tagColor : TagColor.Transparent;

  public event TypedEventHandler<TagColorSelector, TagColorChangedEventArgs>? TagColorChanged;
}

internal class TagColorChangedEventArgs(TagColor oldColor, TagColor newColor) : EventArgs
{
  public TagColor? OldColor { get; } = oldColor;
  public TagColor NewColor { get; } = newColor;
}
