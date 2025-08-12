namespace MyNotes.Common.Converters;

internal class BoolToVisibilityConverter : DependencyObject, IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, string language)
  {
    if (value is bool boolValue)
    {
      if (IsInverted)
        return boolValue ? Visibility.Collapsed : Visibility.Visible;
      else
        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }
    else
      throw new ArgumentException("");
  }

  public object ConvertBack(object value, Type targetType, object parameter, string language)
  {
    if(value is Visibility visibilityValue)
      return IsInverted ? visibilityValue == Visibility.Collapsed : visibilityValue == Visibility.Visible;
    else
      throw new ArgumentException("");
  }

  public static readonly DependencyProperty IsInvertedProperty = DependencyProperty.Register("IsInverted", typeof(bool), typeof(BoolToVisibilityConverter), new PropertyMetadata(false));
  public bool IsInverted
  {
    get => (bool)GetValue(IsInvertedProperty);
    set => SetValue(IsInvertedProperty, value);
  }
}
