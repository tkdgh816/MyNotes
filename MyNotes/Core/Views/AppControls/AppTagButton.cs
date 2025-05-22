namespace MyNotes.Core.Views;

public sealed class AppTagButton : Button
{
  public AppTagButton()
  {
    this.DefaultStyleKey = typeof(AppTagButton);
  }

  public Button DeleteButton { get; private set; } = null!;

  protected override void OnApplyTemplate()
  {
    base.OnApplyTemplate();
    DeleteButton = (Button)GetTemplateChild("DeleteButton");
    DeleteButton.Click += (s, e) => DeleteButtonClick?.Invoke(this, e);
  }

  public static readonly DependencyProperty RemoveButtonVisibleProperty = DependencyProperty.Register("RemoveButtonVisible", typeof(Visibility), typeof(AppTagButton), new PropertyMetadata(Visibility.Visible));
  public Visibility RemoveButtonVisible
  {
    get => (Visibility)GetValue(RemoveButtonVisibleProperty);
    set => SetValue(RemoveButtonVisibleProperty, value);
  }

  public event RoutedEventHandler? DeleteButtonClick;
}
