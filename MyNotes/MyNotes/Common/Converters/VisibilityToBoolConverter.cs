namespace MyNotes.Common.Converters;

internal class VisibilityToBoolConverter : IValueConverter
{
  public static bool Convert(object value)
    => value is Visibility visibilityValue && visibilityValue == Visibility.Visible;

  public static Visibility ConvertBack(object value)
  => (value is bool boolValue && boolValue) ? Visibility.Visible : Visibility.Collapsed;

  public object Convert(object value, Type targetType, object parameter, string language)
    => Convert(value);

  public object ConvertBack(object value, Type targetType, object parameter, string language)
    => ConvertBack(value);
}
