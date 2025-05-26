namespace MyNotes.Common.Converters;
public class ColorToLayerBrushConverter : IValueConverter
{
  public static SolidColorBrush Convert(object value)
  {
    if (value is Color color)
    {
      return (color.A < 64) ? _colorBurshDark
        : GetRelativeLuminance(color) switch
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
  private static double GetRelativeLuminance(Color c)
  {
    var rSRGB = c.R / 255.0;
    var gSRGB = c.G / 255.0;
    var bSRGB = c.B / 255.0;

    var r = rSRGB <= 0.04045 ? rSRGB / 12.92 : Math.Pow((rSRGB + 0.055) / 1.055, 2.4);
    var g = gSRGB <= 0.04045 ? gSRGB / 12.92 : Math.Pow((gSRGB + 0.055) / 1.055, 2.4);
    var b = bSRGB <= 0.04045 ? bSRGB / 12.92 : Math.Pow((bSRGB + 0.055) / 1.055, 2.4);
    return 0.2126 * r + 0.7152 * g + 0.0722 * b;
  }
}
