namespace MyNotes.Core.View;
internal sealed partial class HomePage : Page
{
  public HomePage()
  {
    this.InitializeComponent();
  }

  public double CalculateContrastRatio(Color first, Color second)
  {
    var relLuminanceOne = GetRelativeLuminance(first);
    var relLuminanceTwo = GetRelativeLuminance(second);
    return (Math.Max(relLuminanceOne, relLuminanceTwo) + 0.05)
        / (Math.Min(relLuminanceOne, relLuminanceTwo) + 0.05);
  }

  public double GetRelativeLuminance(Color c)
  {
    var rSRGB = c.R / 255.0;
    var gSRGB = c.G / 255.0;
    var bSRGB = c.B / 255.0;

    var r = rSRGB <= 0.04045 ? rSRGB / 12.92 : Math.Pow((rSRGB + 0.055) / 1.055, 2.4);
    var g = gSRGB <= 0.04045 ? gSRGB / 12.92 : Math.Pow((gSRGB + 0.055) / 1.055, 2.4);
    var b = bSRGB <= 0.04045 ? bSRGB / 12.92 : Math.Pow((bSRGB + 0.055) / 1.055, 2.4);
    return 0.2126 * r + 0.7152 * g + 0.0722 * b;
  }

  private void ColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
  {
    CheckContrast();
  }

  private void CheckContrast()
  {
    double ratio = CalculateContrastRatio(BackgroundColorPicker.Color, ForegroundColorPicker.Color);
    ContrastTestTextBlock.Text = $"{Math.Round(ratio, 2)}";
    RegularTestTextBlock.Text = (ratio >= 4.5) ? "Pass" : "Fail";
    RegularTestTextBlock.Foreground = (ratio >= 4.5) ? new SolidColorBrush(Colors.DarkGreen) : new SolidColorBrush(Colors.DarkRed);
    LargeTestTextBlock.Text = (ratio >= 3.0) ? "Pass" : "Fail";
    LargeTestTextBlock.Foreground = (ratio >= 3.0) ? new SolidColorBrush(Colors.DarkGreen) : new SolidColorBrush(Colors.DarkRed);
    double luminance = GetRelativeLuminance(BackdropColorPicker.Color);
    LuminanceTestTextBlock.Text = $"{Math.Round(luminance, 2)}";
  }
}
