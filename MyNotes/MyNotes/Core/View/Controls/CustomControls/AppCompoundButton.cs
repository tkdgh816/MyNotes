namespace MyNotes.Core.View;

public sealed class AppCompoundButton : Button
{
  public AppCompoundButton()
  {
    this.DefaultStyleKey = typeof(AppCompoundButton);
  }

  public static bool IsTemplateApplied { get; private set; } = false;
  public Grid View_RootGrid { get; private set; } = null!;

  protected override void OnApplyTemplate()
  {
    base.OnApplyTemplate();
    View_RootGrid = (Grid)GetTemplateChild("RootGrid");
    IsTemplateApplied = true;
    OnVisibilityChanged(this);
  }

  public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(IconElement), typeof(AppCompoundButton), new PropertyMetadata(null));
  public IconElement Icon
  {
    get => (IconElement)GetValue(IconProperty);
    set => SetValue(IconProperty, value);
  }

  public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register("Spacing", typeof(double), typeof(AppCompoundButton), new PropertyMetadata(16.0));
  public double Spacing
  {
    get => (double)GetValue(SpacingProperty);
    set => SetValue(SpacingProperty, value);
  }

  public static readonly DependencyProperty IsIconVisibleProperty = DependencyProperty.Register("IsIconVisible", typeof(Visibility), typeof(AppCompoundButton), new PropertyMetadata(Visibility.Visible, OnVisibilityChanged));
  public Visibility IsIconVisible
  {
    get => (Visibility)GetValue(IsIconVisibleProperty);
    set => SetValue(IsIconVisibleProperty, value);
  }

  public static readonly DependencyProperty IsContentVisibleProperty = DependencyProperty.Register("IsContentVisible", typeof(Visibility), typeof(AppCompoundButton), new PropertyMetadata(Visibility.Visible, OnVisibilityChanged));
  public Visibility IsContentVisible
  {
    get => (Visibility)GetValue(IsContentVisibleProperty);
    set => SetValue(IsContentVisibleProperty, value);
  }

  private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (IsTemplateApplied)
      OnVisibilityChanged((AppCompoundButton)d);
  }

  private static void OnVisibilityChanged(AppCompoundButton obj)
    => obj.View_RootGrid.ColumnSpacing = (obj.IsIconVisible == Visibility.Visible && obj.IsContentVisible == Visibility.Visible && obj.Icon != null && obj.Content != null) ? obj.Spacing : 0;
}
