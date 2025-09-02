namespace MyNotes.Common.Helpers;

internal static class ColorHelper
{
  public const double WCAG_AA_Normal = 4.5;
  public const double WCAG_AA_Large = 3.0;
  public const double WCAG_AAA_Narmal = 7.0;
  public const double WCAG_AAA_Large = 4.5;

  public static double GetRelativeLuminance(Color c)
  {
    var rSRGB = c.R / 255.0;
    var gSRGB = c.G / 255.0;
    var bSRGB = c.B / 255.0;

    var r = rSRGB <= 0.04045 ? rSRGB / 12.92 : Math.Pow((rSRGB + 0.055) / 1.055, 2.4);
    var g = gSRGB <= 0.04045 ? gSRGB / 12.92 : Math.Pow((gSRGB + 0.055) / 1.055, 2.4);
    var b = bSRGB <= 0.04045 ? bSRGB / 12.92 : Math.Pow((bSRGB + 0.055) / 1.055, 2.4);
    return 0.2126 * r + 0.7152 * g + 0.0722 * b;
  }

  public static double GetContrastRatio(Color c1, Color c2)
  {
    var relLuminance1 = GetRelativeLuminance(c1);
    var relLuminance2 = GetRelativeLuminance(c2);
    return (Math.Max(relLuminance1, relLuminance2) + 0.05)
        / (Math.Min(relLuminance1, relLuminance2) + 0.05);
  }

  public static Color GetComplementary(Color c) => Color.FromArgb(255, (byte)(255 - c.R), (byte)(255 - c.G), (byte)(255 - c.B));

  public static Color GetAlphaBlendingColor(Color color, Color backgroundColor)
  {
    double alpha = color.A / 255.0;

    return new Color()
    {
      A = 255,
      R = (byte)(color.R * alpha + backgroundColor.R * (1 - alpha)),
      G = (byte)(color.G * alpha + backgroundColor.G * (1 - alpha)),
      B = (byte)(color.B * alpha + backgroundColor.B * (1 - alpha))
    };
  }

  public static Color AdjustBrightness(Color c, double factor) => new Color()
  {
    A = 255,
    R = (byte)Math.Clamp(c.R * factor, 0, 255),
    G = (byte)Math.Clamp(c.G * factor, 0, 255),
    B = (byte)Math.Clamp(c.B * factor, 0, 255)
  };
}
