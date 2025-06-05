namespace MyNotes.Common.Converters;

internal class TypeToObjectConverter : DependencyObject, IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, string language)
  {
    if (value is null)
      return FallbackValue;

    bool typeMatches = AllowBaseType ? TargetType.IsAssignableFrom(value.GetType()) : TargetType.Equals(value.GetType());
    return typeMatches ? MatchedValue : FallbackValue;
  }

  public object ConvertBack(object value, Type targetType, object parameter, string language)
    => throw new NotImplementedException();

  public static readonly DependencyProperty MatchedValueProperty = DependencyProperty.Register("MatchedValue", typeof(object), typeof(TypeToObjectConverter), new PropertyMetadata(null));
  public object MatchedValue
  {
    get => GetValue(MatchedValueProperty);
    set => SetValue(MatchedValueProperty, value);
  }

  public static readonly DependencyProperty FallbackValueProperty = DependencyProperty.Register("FallbackValue", typeof(object), typeof(TypeToObjectConverter), new PropertyMetadata(null));
  public object FallbackValue
  {
    get => GetValue(FallbackValueProperty);
    set => SetValue(FallbackValueProperty, value);
  }

  public static readonly DependencyProperty TargetTypeProperty = DependencyProperty.Register("Type", typeof(Type), typeof(TypeToObjectConverter), new PropertyMetadata(typeof(object)));
  public Type TargetType
  {
    get => (Type)GetValue(TargetTypeProperty);
    set => SetValue(TargetTypeProperty, value);
  }

  public static readonly DependencyProperty AllowBaseTypeProperty = DependencyProperty.Register("AllowBaseType", typeof(bool), typeof(TypeToObjectConverter), new PropertyMetadata(true));
  public bool AllowBaseType
  {
    get => (bool)GetValue(AllowBaseTypeProperty);
    set => SetValue(AllowBaseTypeProperty, value);
  }
}
