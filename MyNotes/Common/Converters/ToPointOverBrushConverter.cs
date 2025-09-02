using ToolkitColorHelper = CommunityToolkit.WinUI.Helpers.ColorHelper;

namespace MyNotes.Common.Converters;

internal class ToPointOverBrushConverter : IValueConverter
{
  public static SolidColorBrush Convert(object value)
  {
    SolidColorBrush brush = value is SolidColorBrush b ? b : (value is Color c ? new SolidColorBrush(c) : throw new ArgumentException());
    Color color = brush.Color;
    
    var hsv = ToolkitColorHelper.ToHsv(color);
    if (hsv.H == 0 && hsv.S == 0)
      hsv.V -= 0.08;
    else if (hsv.V > 0.08)
      hsv.S += 0.08;
    else
      hsv.A += 0.08;

    color = ToolkitColorHelper.FromHsv(hsv.H, hsv.S, hsv.V, hsv.A);
    return new SolidColorBrush(color);
  }

  public object Convert(object value, Type targetType, object parameter, string language)
    => Convert(value);

  public object ConvertBack(object value, Type targetType, object parameter, string language)
    => throw new NotImplementedException();
}
