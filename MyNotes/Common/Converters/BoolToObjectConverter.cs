using Microsoft.UI.Xaml.Media.Imaging;

namespace MyNotes.Common.Converters;

public class BoolToObjectConverter : DependencyObject, IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, string language)
  {
    if (value is bool boolValue)
    {
      //object trueValueConversion = TrueValue;
      //object falseValueConversion = FalseValue;
      //if (targetType.Equals(typeof(Uri)))
      //{
      //  trueValueConversion = new Uri((string)TrueValue);
      //  falseValueConversion = new Uri((string)FalseValue);
      //}
      //return boolValue ? trueValueConversion : falseValueConversion;
      return boolValue ? TrueValue : FalseValue;
    }
    else
      throw new ArgumentException();
  }

  public object ConvertBack(object value, Type targetType, object parameter, string language)
    => throw new NotImplementedException();

  public static readonly DependencyProperty TrueValueProperty = DependencyProperty.Register("TrueValue", typeof(object), typeof(BoolToObjectConverter), new PropertyMetadata(null));
  public object TrueValue
  {
    get => GetValue(TrueValueProperty);
    set => SetValue(TrueValueProperty, value);
  }

  public static readonly DependencyProperty FalseValueProperty = DependencyProperty.Register("FalseValue", typeof(object), typeof(BoolToObjectConverter), new PropertyMetadata(null));
  public object FalseValue
  {
    get => GetValue(FalseValueProperty);
    set => SetValue(FalseValueProperty, value);
  }
}
