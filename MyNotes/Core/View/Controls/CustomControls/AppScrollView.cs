namespace MyNotes.Core.View;

[ContentProperty(Name = "Content")]

public sealed class AppScrollView : Control
{
  public AppScrollView()
  {
    this.DefaultStyleKey = typeof(AppScrollView);
  }

  ScrollView ContentScrollView = null!;
  RepeatButton TopButton = null!, BottomButton = null!, LeftButton = null!, RightButton = null!;

  protected override void OnApplyTemplate()
  {
    base.OnApplyTemplate();
    ContentScrollView = (ScrollView)GetTemplateChild("ContentScrollView");
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

    ContentScrollView.ViewChanged += (s, e) => SetScrollButtonsState();

    ScrollingScrollOptions scrollingScrollOptions = new(ScrollingAnimationMode.Auto, ScrollingSnapPointsMode.Ignore);
    TopButton.Click += (s, e) =>
    {
      ContentScrollView.ScrollTo(0.0, Math.Clamp(ContentScrollView.VerticalOffset - ScrollInterval, 0.0, ContentScrollView.ScrollableHeight), scrollingScrollOptions);
      //ContentScrollView.ScrollBy(0.0, -ScrollInterval);
    };
    BottomButton.Click += (s, e) =>
    {
      ContentScrollView.ScrollTo(0.0, Math.Clamp(ContentScrollView.VerticalOffset + ScrollInterval, 0.0, ContentScrollView.ScrollableHeight), scrollingScrollOptions);
      //ContentScrollView.ScrollBy(0.0, ScrollInterval);
    };
    LeftButton.Click += (s, e) =>
    {
      ContentScrollView.ScrollTo(Math.Clamp(ContentScrollView.HorizontalOffset - ScrollInterval, 0.0, ContentScrollView.ScrollableWidth), 0.0, scrollingScrollOptions);
      //ContentScrollView.ScrollBy(-ScrollInterval, 0.0);
    };
    RightButton.Click += (s, e) =>
    {
      ContentScrollView.ScrollTo(Math.Clamp(ContentScrollView.HorizontalOffset + ScrollInterval, 0.0, ContentScrollView.ScrollableWidth), 0.0, scrollingScrollOptions);
      //ContentScrollView.ScrollBy(ScrollInterval, 0.0);
    };
  }

  private void SetScrollButtonsState()
  {
    double scrollableWidth = ContentScrollView.ScrollableWidth;
    double scrollableHeight = ContentScrollView.ScrollableHeight;
    double horizontalOffset = ContentScrollView.HorizontalOffset;
    double verticalOffset = ContentScrollView.VerticalOffset;

    if (HorizontalScrollMode != ScrollingScrollMode.Disabled)
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

    if (VerticalScrollMode != ScrollingScrollMode.Disabled)
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

  public static readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(AppScrollView), new PropertyMetadata(null));
  public object Content
  {
    get => GetValue(ContentProperty);
    set => SetValue(ContentProperty, value);
  }

  public static readonly DependencyProperty HorizontalScrollModeProperty = DependencyProperty.Register("HorizontalScrollMode", typeof(ScrollingScrollMode), typeof(AppScrollView), new PropertyMetadata(ScrollingScrollMode.Auto));
  public ScrollingScrollMode HorizontalScrollMode
  {
    get => (ScrollingScrollMode)GetValue(HorizontalScrollModeProperty);
    set => SetValue(HorizontalScrollModeProperty, value);
  }

  public static readonly DependencyProperty VerticalScrollModeProperty = DependencyProperty.Register("VerticalScrollMode", typeof(ScrollingScrollMode), typeof(AppScrollView), new PropertyMetadata(ScrollingScrollMode.Auto));
  public ScrollingScrollMode VerticalScrollMode
  {
    get => (ScrollingScrollMode)GetValue(VerticalScrollModeProperty);
    set => SetValue(VerticalScrollModeProperty, value);
  }

  public static readonly DependencyProperty ContentOrientationProperty = DependencyProperty.Register("ContentOrientation", typeof(ScrollingContentOrientation), typeof(AppScrollView), new PropertyMetadata(ScrollingContentOrientation.Vertical));
  public ScrollingContentOrientation ContentOrientation
  {
    get => (ScrollingContentOrientation)GetValue(ContentOrientationProperty);
    set => SetValue(ContentOrientationProperty, value);
  }

  public static readonly DependencyProperty ScrollIntervalProperty = DependencyProperty.Register("ScrollInterval", typeof(double), typeof(AppScrollView), new PropertyMetadata(20.0));
  public double ScrollInterval
  {
    get => (double)GetValue(ScrollIntervalProperty);
    set => SetValue(ScrollIntervalProperty, value);
  }
}