namespace MyNotes.Common.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
  public static Visibility Convert(bool value) => value ? Visibility.Visible : Visibility.Collapsed;

  public object Convert(object value, Type targetType, object parameter, string language)
  {
    return Convert((bool)value);
  }

  public object ConvertBack(object value, Type targetType, object parameter, string language)
  {
    throw new NotImplementedException();
  }
}
