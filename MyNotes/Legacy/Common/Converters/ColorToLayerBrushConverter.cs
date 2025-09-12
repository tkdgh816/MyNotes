using AppColorHelper = MyNotes.Common.Helpers.ColorHelper;

namespace MyNotes.Common.Converters;

internal class ColorToLayerBrushConverter : IValueConverter
{
  public static SolidColorBrush Convert(object value)
  {
    if (value is Color color)
    {
      return (color.A < 64) ? _colorBurshDark
        : AppColorHelper.GetRelativeLuminance(color) switch
        {
          >= 0 and <= 0.25 => _colorBurshLight,
          > 0.25 and <= 0.5 => _colorBurshMediumLight,
          > 0.5 and <= 0.75 => _colorBurshMediumDark,
          > 0.75 and <= 1.0 => _colorBurshDark,
          _ => _colorBurshDark
        };
    }
    else
      return _colorBurshTransparent;
  }

  public object Convert(object value, Type targetType, object parameter, string language)
    => Convert(value);

  public object ConvertBack(object value, Type targetType, object parameter, string language)
    => throw new NotImplementedException();

  private static SolidColorBrush _colorBurshLight = new(ColorHelper.FromArgb(96, 255, 255, 255));
  private static SolidColorBrush _colorBurshMediumLight = new(ColorHelper.FromArgb(96, 200, 200, 200));
  private static SolidColorBrush _colorBurshMediumDark = new(ColorHelper.FromArgb(16, 16, 16, 16));
  private static SolidColorBrush _colorBurshDark = new(ColorHelper.FromArgb(16, 0, 0, 0));
  private static SolidColorBrush _colorBurshTransparent = new(Colors.Transparent);
}
