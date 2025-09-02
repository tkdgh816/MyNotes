using System.Windows.Input;

using Microsoft.Xaml.Interactivity;

namespace MyNotes.Common.Behaviors;
internal static class EventTrigger
{
  public static readonly DependencyProperty EventNameProperty =
      DependencyProperty.RegisterAttached(
          "EventName",
          typeof(string),
          typeof(EventTrigger),
          new PropertyMetadata(null, OnEventNameChanged));

  public static void SetEventName(DependencyObject element, string value) =>
      element.SetValue(EventNameProperty, value);

  public static string GetEventName(DependencyObject element) =>
      (string)element.GetValue(EventNameProperty);

  public static readonly DependencyProperty CommandProperty =
      DependencyProperty.RegisterAttached(
          "Command",
          typeof(ICommand),
          typeof(EventTrigger),
          new PropertyMetadata(null));

  public static void SetCommand(DependencyObject element, ICommand value) =>
      element.SetValue(CommandProperty, value);

  public static ICommand GetCommand(DependencyObject element) =>
      (ICommand)element.GetValue(CommandProperty);

  public static readonly DependencyProperty CommandParameterProperty =
      DependencyProperty.RegisterAttached(
          "CommandParameter",
          typeof(object),
          typeof(EventTrigger),
          new PropertyMetadata(null));

  public static void SetCommandParameter(DependencyObject element, object value) =>
      element.SetValue(CommandParameterProperty, value);

  public static object GetCommandParameter(DependencyObject element) =>
      element.GetValue(CommandParameterProperty);

  private static void OnEventNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (d is FrameworkElement element && e.NewValue is string eventName)
    {
      var behaviors = Interaction.GetBehaviors(element);

      // 중복 추가 방지
      if (behaviors.OfType<EventTriggerBehavior>().Any(b => b.EventName == eventName))
        return;

      var behavior = new EventTriggerBehavior { EventName = eventName };

      var action = new InvokeCommandAction();

      // BindingProxy처럼 DataContext 전달이 늦을 수 있으므로 Loaded 이후에 바인딩
      element.Loaded += (s, _) =>
      {
        action.Command = GetCommand(d);
        action.CommandParameter = GetCommandParameter(d);
      };

      behavior.Actions.Add(action);
      behaviors.Add(behavior);
    }
  }
}

