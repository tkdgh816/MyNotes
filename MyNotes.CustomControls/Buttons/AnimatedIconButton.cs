using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyNotes.CustomControls.Buttons;

public sealed partial class AnimatedIconButton : Button
{
  public AnimatedIconButton()
  {
    DefaultStyleKey = typeof(AnimatedIconButton);
  }

  public static readonly DependencyProperty AnimatedIconSourceProperty = DependencyProperty.Register("AnimatedIconSource", typeof(IAnimatedVisualSource2), typeof(AnimatedIconButton), new PropertyMetadata(null));
  public IAnimatedVisualSource2 AnimatedIconSource
  {
    get => (IAnimatedVisualSource2)GetValue(AnimatedIconSourceProperty);
    set => SetValue(AnimatedIconSourceProperty, value);
  }

  protected override void OnApplyTemplate()
  {
    base.OnApplyTemplate();
  }
}
