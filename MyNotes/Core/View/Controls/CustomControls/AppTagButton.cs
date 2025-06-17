using ToolkitColorHelper = CommunityToolkit.WinUI.Helpers.ColorHelper;

namespace MyNotes.Core.View;

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

  public static readonly DependencyProperty DeleteButtonVisibleProperty = DependencyProperty.Register("DeleteButtonVisible", typeof(Visibility), typeof(AppTagButton), new PropertyMetadata(Visibility.Visible));
  public Visibility DeleteButtonVisible
  {
    get => (Visibility)GetValue(DeleteButtonVisibleProperty);
    set => SetValue(DeleteButtonVisibleProperty, value);
  }

  public event RoutedEventHandler? DeleteButtonClick;

  public Brush PointerOverBrush
  {
    get
    {
      if (Background is SolidColorBrush brush)
      {
        Color color = brush.Color;

        var hsv = ToolkitColorHelper.ToHsv(color);
        if (hsv.V > 0.08)
          hsv.S += 0.08;
        else
          hsv.A += 0.08;

        color = ToolkitColorHelper.FromHsv(hsv.H, hsv.S, hsv.V, hsv.A);
        return new SolidColorBrush(color);
      }

      return Background;
    }
  }
}
