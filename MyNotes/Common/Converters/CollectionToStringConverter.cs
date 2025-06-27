namespace MyNotes.Common.Converters;
internal class CollectionToStringConverter : IValueConverter
{
  public static object Convert(object value, object parameter)
    => value is IEnumerable collection ? string.Join(", ", collection.Cast<object>().Select(obj => obj.ToString())) : "";

  public object Convert(object value, Type targetType, object parameter, string language)
    => Convert(value, parameter);

  public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
