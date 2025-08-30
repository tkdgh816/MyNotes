namespace MyNotes.Common.Converters;

internal class EmptyCollectionToObjectConverter : DependencyObject, IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, string language)
    => value is IEnumerable enumerable ? (enumerable.GetEnumerator().MoveNext() ? TrueValue : FalseValue) : throw new ArgumentException();

  public object ConvertBack(object value, Type targetType, object parameter, string language)
    => throw new NotImplementedException();

  public static readonly DependencyProperty TrueValueProperty = DependencyProperty.Register("TrueValue", typeof(object), typeof(EmptyCollectionToObjectConverter), new PropertyMetadata(null));
  public object TrueValue
  {
    get => GetValue(TrueValueProperty);
    set => SetValue(TrueValueProperty, value);
  }

  public static readonly DependencyProperty FalseValueProperty = DependencyProperty.Register("FalseValue", typeof(object), typeof(EmptyCollectionToObjectConverter), new PropertyMetadata(null));
  public object FalseValue
  {
    get => GetValue(FalseValueProperty);
    set => SetValue(FalseValueProperty, value);
  }

}
