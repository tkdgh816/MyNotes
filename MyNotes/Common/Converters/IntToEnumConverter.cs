namespace MyNotes.Common.Converters;
internal class IntToEnumConverter : DependencyObject, IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, string language)
    => value is int intValue && EnumType.IsEnum ? Enum.ToObject(EnumType, intValue) : throw new ArgumentException("");

  public object? ConvertBack(object value, Type targetType, object parameter, string language)
    => throw new NotImplementedException();

  public static readonly DependencyProperty EnumTypeProperty = DependencyProperty.Register("EnumType", typeof(Type), typeof(IntToEnumConverter), new PropertyMetadata(null));
  public Type EnumType
  {
    get => (Type)GetValue(EnumTypeProperty);
    set => SetValue(EnumTypeProperty, value);
  }
}
