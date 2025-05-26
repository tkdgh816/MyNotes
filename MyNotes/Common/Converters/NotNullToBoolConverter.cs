namespace MyNotes.Common.Converters;

public class NotNullToBoolConverter : IValueConverter
{
  public static bool Convert(object value)
    => value is not null;

  public object Convert(object value, Type targetType, object parameter, string language)
    => Convert(value);

  public object ConvertBack(object value, Type targetType, object parameter, string language) =>
    throw new NotImplementedException();
}
