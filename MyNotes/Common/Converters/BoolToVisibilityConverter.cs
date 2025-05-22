namespace MyNotes.Common.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
  public static Visibility Convert(object value)
    => (value is bool boolValue && boolValue) ? Visibility.Visible : Visibility.Collapsed;

  public static bool ConvertBack(object value)
    => value is Visibility visibilityValue && visibilityValue == Visibility.Visible;

  public object Convert(object value, Type targetType, object parameter, string language)
    => Convert(value);

  public object ConvertBack(object value, Type targetType, object parameter, string language)
    => ConvertBack(value);
}
