namespace MyNotes.Core.Views;

public sealed class IconButton : Button
{
  public IconButton()
  {
    this.DefaultStyleKey = typeof(IconButton);
  }

  public static bool IsTemplateApplied { get; private set; } = false;
  public Grid VIEW_RootGrid { get; private set; } = null!;

  protected override void OnApplyTemplate()
  {
    base.OnApplyTemplate();
    VIEW_RootGrid = (Grid)GetTemplateChild("RootGrid");
    IsTemplateApplied = true;
    OnVisibilityChanged(this);
  }

  public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(IconElement), typeof(IconButton), new PropertyMetadata(null));
  public IconElement Icon
  {
    get => (IconElement)GetValue(IconProperty);
    set => SetValue(IconProperty, value);
  }

  public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register("Spacing", typeof(double), typeof(IconButton), new PropertyMetadata(16.0));
  public double Spacing
  {
    get => (double)GetValue(SpacingProperty);
    set => SetValue(SpacingProperty, value);
  }

  public static readonly DependencyProperty IsIconVisibleProperty = DependencyProperty.Register("IsIconVisible", typeof(Visibility), typeof(IconButton), new PropertyMetadata(Visibility.Visible, OnVisibilityChanged));
  public Visibility IsIconVisible
  {
    get => (Visibility)GetValue(IsIconVisibleProperty);
    set => SetValue(IsIconVisibleProperty, value);
  }

  public static readonly DependencyProperty IsContentVisibleProperty = DependencyProperty.Register("IsContentVisible", typeof(Visibility), typeof(IconButton), new PropertyMetadata(Visibility.Visible, OnVisibilityChanged));
  public Visibility IsContentVisible
  {
    get => (Visibility)GetValue(IsContentVisibleProperty);
    set => SetValue(IsContentVisibleProperty, value);
  }

  private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (IsTemplateApplied)
      OnVisibilityChanged((IconButton)d);
  }

  private static void OnVisibilityChanged(IconButton obj)
    => obj.VIEW_RootGrid.ColumnSpacing = (obj.IsIconVisible == Visibility.Visible && obj.IsContentVisible == Visibility.Visible && obj.Icon != null && obj.Content != null) ? obj.Spacing : 0;
}
