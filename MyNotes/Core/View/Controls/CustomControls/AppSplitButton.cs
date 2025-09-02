namespace MyNotes.Core.View;

public sealed class AppSplitButton : Button
{
  public AppSplitButton()
  {
    this.DefaultStyleKey = typeof(AppSplitButton);
  }

  Button SecondaryButton = null!;
  AnimatedIcon SecondaryButtonAnimatedIcon = null!;

  protected override void OnApplyTemplate()
  {
    base.OnApplyTemplate();
    SecondaryButton = (Button)GetTemplateChild("SecondaryButton");
    SecondaryButtonAnimatedIcon = (AnimatedIcon)GetTemplateChild("SecondaryButtonAnimatedIcon");

    SecondaryButton.PointerEntered += (s, e) => AnimatedIcon.SetState(SecondaryButtonAnimatedIcon, "PointerOver");
    SecondaryButton.PointerExited += (s, e) => AnimatedIcon.SetState(SecondaryButtonAnimatedIcon, "Normal");
    SecondaryButton.Click += (s, e) => AnimatedIcon.SetState(SecondaryButtonAnimatedIcon, "Pressed");
  }

  public static readonly DependencyProperty SecondaryButtonFlyoutProperty = DependencyProperty.Register("SecondaryButtonFlyout", typeof(Flyout), typeof(AppSplitButton), new PropertyMetadata(null));
  public Flyout SecondaryButtonFlyout
  {
    get => (Flyout)GetValue(SecondaryButtonFlyoutProperty);
    set => SetValue(SecondaryButtonFlyoutProperty, value);
  }
}
