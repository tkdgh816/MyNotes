namespace MyNotes.Common.Converters;

internal class StringToUriConverter : IValueConverter
{
  public static object Convert(object value)
    => value is string stringValue ? new Uri(stringValue) : new Uri("");

  public static object ConvertBack(object value)
    => value is Uri uriValue ? uriValue.ToString() : "";

  public object Convert(object value, Type targetType, object parameter, string language)
    => Convert(value);

  public object ConvertBack(object value, Type targetType, object parameter, string language)
    => ConvertBack(value);
}
