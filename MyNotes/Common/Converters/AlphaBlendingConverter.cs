namespace MyNotes.Common.Converters;

internal class AlphaBlendingConverter : DependencyObject, IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, string language)
  {
    if(value is Color color)
    {
      double alpha = color.A / 255.0;

      Color newColor = new() { A = 255 };
      newColor.R = (byte)(color.R * alpha + BackgroundColor.R * (1 - alpha));
      newColor.G = (byte)(color.G * alpha + BackgroundColor.G * (1 - alpha));
      newColor.B = (byte)(color.B * alpha + BackgroundColor.B * (1 - alpha));

      return new SolidColorBrush(newColor);
    }
    throw new ArgumentException();
  }

  public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();

  public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register("BackgroundColor", typeof(Color), typeof(AlphaBlendingConverter), new PropertyMetadata(Colors.White));
  public Color BackgroundColor
  {
    get => (Color)GetValue(BackgroundColorProperty);
    set => SetValue(BackgroundColorProperty, value);
  }
}
