namespace MyNotes.Common.Converters;

public class BoolNegationConverter : IValueConverter
{
  public static bool Convert(object value) => !(value is bool boolValue && boolValue);

  public object Convert(object value, Type targetType, object parameter, string language)
    => Convert(value);

  public object ConvertBack(object value, Type targetType, object parameter, string language)
    => Convert(value);
}
