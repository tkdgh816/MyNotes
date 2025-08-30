namespace MyNotes.Common.Converters;

internal class EmptyStringToBoolConverter : IValueConverter
{
  public static bool Convert(object value)
    => value is string stringValue && !string.IsNullOrWhiteSpace(stringValue);

  public object Convert(object value, Type targetType, object parameter, string language)
    => Convert((string)value);

  public object ConvertBack(object value, Type targetType, object parameter, string language)=>
    throw new NotImplementedException();
}
