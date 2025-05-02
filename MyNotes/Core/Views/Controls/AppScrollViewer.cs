namespace MyNotes.Core.Views;

[ContentProperty(Name = "Content")]

public sealed class AppScrollViewer : Control
{
  public AppScrollViewer()
  {
    this.DefaultStyleKey = typeof(AppScrollViewer);
  }

  ScrollViewer ContentScrollViewer = null!;
  Button TopButton = null!, BottomButton = null!, LeftButton = null!, RightButton = null!;

  protected override void OnApplyTemplate()
  {
    base.OnApplyTemplate();
    ContentScrollViewer = (ScrollViewer)GetTemplateChild("ContentScrollViewer");
    TopButton = (Button)GetTemplateChild("TopButton");
    BottomButton = (Button)GetTemplateChild("BottomButton");
    LeftButton = (Button)GetTemplateChild("LeftButton");
    RightButton = (Button)GetTemplateChild("RightButton");

    ContentScrollViewer.ViewChanged += (s, e) => SetScrollButtonsState();
    ContentScrollViewer.SizeChanged += (s, e) => SetScrollButtonsState();
    TopButton.Click += (s, e) => ContentScrollViewer.ChangeView(null, ContentScrollViewer.VerticalOffset - ScrollInterval, null);
    BottomButton.Click += (s, e) => ContentScrollViewer.ChangeView(null, ContentScrollViewer.VerticalOffset + ScrollInterval, null);
    LeftButton.Click += (s, e) => ContentScrollViewer.ChangeView(ContentScrollViewer.HorizontalOffset - ScrollInterval, null, null);
    RightButton.Click += (s, e) => ContentScrollViewer.ChangeView(ContentScrollViewer.HorizontalOffset + ScrollInterval, null, null);
  }

  public static readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(AppScrollViewer), new PropertyMetadata(null));
  public object Content
  {
    get => GetValue(ContentProperty);
    set => SetValue(ContentProperty, value);
  }

  public static readonly DependencyProperty HorizontalScrollModeProperty = DependencyProperty.Register("HorizontalScrollMode", typeof(ScrollMode), typeof(AppScrollViewer), new PropertyMetadata(ScrollMode.Auto));
  public ScrollMode HorizontalScrollMode
  {
    get => (ScrollMode)GetValue(HorizontalScrollModeProperty);
    set => SetValue(HorizontalScrollModeProperty, value);
  }

  public static readonly DependencyProperty VerticalScrollModeProperty = DependencyProperty.Register("VerticalScrollMode", typeof(ScrollMode), typeof(AppScrollViewer), new PropertyMetadata(ScrollMode.Auto));
  public ScrollMode VerticalScrollMode
  {
    get => (ScrollMode)GetValue(VerticalScrollModeProperty);
    set => SetValue(VerticalScrollModeProperty, value);
  }


  private void SetScrollButtonsState()
  {
    double width = ContentScrollViewer.ScrollableWidth;
    double height = ContentScrollViewer.ScrollableHeight;
    double horizontalOffset = ContentScrollViewer.HorizontalOffset;
    double verticalOffset = ContentScrollViewer.VerticalOffset;

    if (HorizontalScrollMode != ScrollMode.Disabled && width > 0)
    {
      if (horizontalOffset == 0)
        VisualStateManager.GoToState(this, "HorizontalScrollStart", false);
      else if (horizontalOffset == width)
        VisualStateManager.GoToState(this, "HorizontalScrollEnd", false);
      else
        VisualStateManager.GoToState(this, "HorizontalScrollMiddle", false);
    }
    else
      VisualStateManager.GoToState(this, "HorizontalNoScroll", false);

    if (VerticalScrollMode != ScrollMode.Disabled && height >= 0)
    {
      if (verticalOffset == 0)
        VisualStateManager.GoToState(this, "VerticalScrollStart", false);
      else if (verticalOffset == width)
        VisualStateManager.GoToState(this, "VerticalScrollEnd", false);
      else
        VisualStateManager.GoToState(this, "VerticalScrollMiddle", false);
    }
    else
      VisualStateManager.GoToState(this, "VerticalNoScroll", false);
  }

  public static readonly DependencyProperty ScrollIntervalProperty = DependencyProperty.Register("ScrollInterval", typeof(double), typeof(AppScrollViewer), new PropertyMetadata(50.0));
  public double ScrollInterval
  {
    get => (double)GetValue(ScrollIntervalProperty);
    set => SetValue(ScrollIntervalProperty, value);
  }
}
