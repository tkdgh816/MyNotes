namespace MyNotes.Core.Views;

public sealed class AppNavigationViewItem : NavigationViewItem
{
  public AppNavigationViewItem()
  {
    this.DefaultStyleKey = typeof(AppNavigationViewItem);
  }

  NavigationViewItemPresenter presenter = null!;

  protected override void OnApplyTemplate()
  {
    base.OnApplyTemplate();
    presenter = (NavigationViewItemPresenter)GetTemplateChild("NavigationViewItemPresenter");
    presenter.DragStarting += (sender, args) => PresenterDragStarting?.Invoke(this, args);
    presenter.DropCompleted += (sender, args) => PresenterDropCompleted?.Invoke(this, args);
  }

  public event TypedEventHandler<UIElement, DragStartingEventArgs>? PresenterDragStarting;
  public event TypedEventHandler<UIElement, DropCompletedEventArgs>? PresenterDropCompleted;
}
