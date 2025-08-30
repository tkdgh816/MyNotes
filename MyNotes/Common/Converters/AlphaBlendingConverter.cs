using AppColorHelper = MyNotes.Common.Helpers.ColorHelper;

namespace MyNotes.Common.Converters;

internal class AlphaBlendingConverter : DependencyObject, IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, string language) => value is Color color
      ? new SolidColorBrush(AppColorHelper.GetAlphaBlendingColor(color, BackgroundColor))
      : throw new ArgumentException();

  public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();

  public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register("BackgroundColor", typeof(Color), typeof(AlphaBlendingConverter), new PropertyMetadata(Colors.White));
  public Color BackgroundColor
  {
    get => (Color)GetValue(BackgroundColorProperty);
    set => SetValue(BackgroundColorProperty, value);
  }
}
