namespace MyNotes.Core.View;

[ContentProperty(Name = "Content")]

public sealed class AppScrollViewer : Control
{
  public AppScrollViewer()
  {
    this.DefaultStyleKey = typeof(AppScrollViewer);
  }

  ScrollViewer ContentScrollViewer = null!;
  RepeatButton TopButton = null!, BottomButton = null!, LeftButton = null!, RightButton = null!;

  protected override void OnApplyTemplate()
  {
    base.OnApplyTemplate();
    ContentScrollViewer = (ScrollViewer)GetTemplateChild("ContentScrollViewer");
    TopButton = (RepeatButton)GetTemplateChild("TopButton");
    BottomButton = (RepeatButton)GetTemplateChild("BottomButton");
    LeftButton = (RepeatButton)GetTemplateChild("LeftButton");
    RightButton = (RepeatButton)GetTemplateChild("RightButton");

    this.PointerEntered += (s, e) => SetScrollButtonsState();
    this.PointerExited += (s, e) =>
    {
      VisualStateManager.GoToState(this, "HorizontalNoScroll", false);
      VisualStateManager.GoToState(this, "VerticalNoScroll", false);
    };

    ContentScrollViewer.ViewChanged += (s, e) => SetScrollButtonsState();
    TopButton.Click += (s, e) =>
    {
      ContentScrollViewer.ChangeView(null, Math.Clamp(ContentScrollViewer.VerticalOffset - ScrollInterval, 0.0, ContentScrollViewer.ScrollableHeight), null);
    };
    BottomButton.Click += (s, e) =>
    {
      ContentScrollViewer.ChangeView(null, Math.Clamp(ContentScrollViewer.VerticalOffset + ScrollInterval, 0.0, ContentScrollViewer.ScrollableHeight), null);
    };
    LeftButton.Click += (s, e) =>
    {
      ContentScrollViewer.ChangeView(Math.Clamp(ContentScrollViewer.HorizontalOffset - ScrollInterval, 0.0, ContentScrollViewer.ScrollableWidth), null, null);
    };
    RightButton.Click += (s, e) =>
    {
      ContentScrollViewer.ChangeView(Math.Clamp(ContentScrollViewer.HorizontalOffset + ScrollInterval, 0.0, ContentScrollViewer.ScrollableWidth), null, null);
    };
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
    double scrollableWidth = ContentScrollViewer.ScrollableWidth;
    double scrollableHeight = ContentScrollViewer.ScrollableHeight;
    double horizontalOffset = ContentScrollViewer.HorizontalOffset;
    double verticalOffset = ContentScrollViewer.VerticalOffset;

    if (HorizontalScrollMode != ScrollMode.Disabled)
    {
      if (scrollableWidth <= 0.0)
        VisualStateManager.GoToState(this, "HorizontalNoScroll", false);
      else if (horizontalOffset <= 0.0)
        VisualStateManager.GoToState(this, "HorizontalScrollStart", false);
      else if (horizontalOffset >= scrollableWidth)
        VisualStateManager.GoToState(this, "HorizontalScrollEnd", false);
      else
        VisualStateManager.GoToState(this, "HorizontalScrollMiddle", false);
    }

    if (VerticalScrollMode != ScrollMode.Disabled)
    {
      if (scrollableHeight <= 0.0)
        VisualStateManager.GoToState(this, "VerticalNoScroll", false);
      else if (verticalOffset <= 0.0)
        VisualStateManager.GoToState(this, "VerticalScrollStart", false);
      else if (verticalOffset >= scrollableHeight)
        VisualStateManager.GoToState(this, "VerticalScrollEnd", false);
      else
        VisualStateManager.GoToState(this, "VerticalScrollMiddle", false);
    }
  }

  public static readonly DependencyProperty ScrollIntervalProperty = DependencyProperty.Register("ScrollInterval", typeof(double), typeof(AppScrollViewer), new PropertyMetadata(20.0));
  public double ScrollInterval
  {
    get => (double)GetValue(ScrollIntervalProperty);
    set => SetValue(ScrollIntervalProperty, value);
  }
}
