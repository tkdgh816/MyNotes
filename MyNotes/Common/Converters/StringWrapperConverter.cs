namespace MyNotes.Common.Converters;

internal class StringWrapperConverter : IValueConverter
{
  public static object Convert(object value, object parameter)
  {
    if (value is string stringValue && parameter is string wrapper)
    {
      var splitted = wrapper.Split("||");
      string prefix = splitted.Length > 0 ? splitted[0] : "";
      string suffix = splitted.Length > 1 ? splitted[1] : "";
      return $"{prefix}{stringValue}{suffix}";
    }
    else
      throw new ArgumentException();
  }
  public object Convert(object value, Type targetType, object parameter, string language)
    => Convert(value, parameter);

  public object ConvertBack(object value, Type targetType, object parameter, string language)
    => throw new NotImplementedException();
}
