namespace MyNotes.Core.View;

public sealed class AppNavigationViewItem : NavigationViewItem
{
  public AppNavigationViewItem()
  {
    this.DefaultStyleKey = typeof(AppNavigationViewItem);
  }

  private NavigationViewItemPresenter _presenter = null!;

  protected override void OnApplyTemplate()
  {
    base.OnApplyTemplate();
    _presenter = (NavigationViewItemPresenter)GetTemplateChild("NavigationViewItemPresenter");

    _presenter.DragStarting += (sender, args) => PresenterDragStarting?.Invoke(this, args);
    _presenter.DropCompleted += (sender, args) => PresenterDropCompleted?.Invoke(this, args);

    this.DragEnter += (sender, args) =>
    {
      if (AllowHoverOverlay)
        VisualStateManager.GoToState(this, "HoverOverlayVisible", false);
    };
    this.DragLeave += (sender, args) => VisualStateManager.GoToState(this, "HoverOverlayCollapsed", false);
    this.Drop += (sender, args) => VisualStateManager.GoToState(this, "HoverOverlayCollapsed", false);
  }

  public event TypedEventHandler<UIElement, DragStartingEventArgs>? PresenterDragStarting;
  public event TypedEventHandler<UIElement, DropCompletedEventArgs>? PresenterDropCompleted;


  public static readonly DependencyProperty AllowHoverOverlayProperty = DependencyProperty.Register("AllowHoverOverlay", typeof(bool), typeof(AppNavigationViewItem), new PropertyMetadata(true));
  public bool AllowHoverOverlay
  {
    get => (bool)GetValue(AllowHoverOverlayProperty);
    set => SetValue(AllowHoverOverlayProperty, value);
  }
}
