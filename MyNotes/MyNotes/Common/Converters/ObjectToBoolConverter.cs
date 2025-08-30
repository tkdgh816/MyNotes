namespace MyNotes.Common.Converters;

class ObjectToBoolConverter : DependencyObject, IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, string language)
    => value is not null && ObjectType.IsAssignableFrom(value.GetType()) && TargetValue.Equals(value);

  public object? ConvertBack(object value, Type targetType, object parameter, string language)
    => value is bool boolValue && boolValue ? TargetValue : Activator.CreateInstance(ObjectType);

  public static readonly DependencyProperty TargetValueProperty = DependencyProperty.Register("TargetValue", typeof(object), typeof(ObjectToBoolConverter), new PropertyMetadata(null, OnTargetValueChanged));
  public object TargetValue
  {
    get => GetValue(TargetValueProperty);
    set => SetValue(TargetValueProperty, value);
  }

  private static void OnTargetValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (!_isObjectTypeLoaded) return;
    ObjectToBoolConverter instance = (ObjectToBoolConverter)d;
    Type objectType = instance.ObjectType;
    if (!objectType.IsAssignableFrom(e.NewValue.GetType()))
      instance.SetValue(TargetValueProperty, Activator.CreateInstance(objectType));
  }

  public static readonly DependencyProperty ObjectTypeProperty = DependencyProperty.Register("Type", typeof(Type), typeof(ObjectToBoolConverter), new PropertyMetadata(typeof(object), OnObjectTypeChanged));
  public Type ObjectType
  {
    get => (Type)GetValue(ObjectTypeProperty);
    set => SetValue(ObjectTypeProperty, value);
  }

  private static bool _isObjectTypeLoaded = false;
  private static void OnObjectTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    => _isObjectTypeLoaded = true;
}