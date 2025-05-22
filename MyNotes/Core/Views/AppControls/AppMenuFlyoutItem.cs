namespace MyNotes.Core.Views;

[ContentProperty(Name = "Content")]
public sealed partial class AppMenuFlyoutItem : MenuFlyoutItem
{
  public AppMenuFlyoutItem()
  {
    DefaultStyleKey = typeof(AppMenuFlyoutItem);
  }

  public static readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(AppMenuFlyoutItem), new PropertyMetadata(null));
  public object Content
  {
    get => GetValue(ContentProperty);
    set => SetValue(ContentProperty, value);
  }
}
