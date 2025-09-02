using System.Windows.Input;

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

  public static readonly DependencyProperty DeleteCommandProperty = DependencyProperty.Register("DeleteCommand", typeof(ICommand), typeof(AppTagButton), new PropertyMetadata(null));
  public ICommand DeleteCommand
  {
    get => (ICommand)GetValue(DeleteCommandProperty);
    set => SetValue(DeleteCommandProperty, value);
  }

  public static readonly DependencyProperty DeleteCommandParameterProperty = DependencyProperty.Register("DeleteCommandParameter", typeof(object), typeof(AppTagButton), new PropertyMetadata(null));
  public object DeleteCommandParameter
  {
    get => GetValue(DeleteCommandParameterProperty);
    set => SetValue(DeleteCommandParameterProperty, value);
  }
}
