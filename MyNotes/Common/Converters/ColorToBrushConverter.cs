namespace MyNotes.Common.Converters;

public class ColorToBrushConverter : IValueConverter
{
  public static SolidColorBrush Convert(object value)
    => value is Color color ? new SolidColorBrush(color) : new SolidColorBrush(Colors.Transparent);

  public static Color ConvertBack(object value)
  => value is SolidColorBrush brush ? brush.Color : Colors.Transparent;

  public object Convert(object value, Type targetType, object parameter, string language)
    => Convert(value);

  public object ConvertBack(object value, Type targetType, object parameter, string language)
    => ConvertBack(value);
}
