using ToolkitColorHelper = CommunityToolkit.WinUI.Helpers.ColorHelper;
using AppColorHelper = MyNotes.Common.Helpers.ColorHelper;

namespace MyNotes.Core.View;
internal sealed partial class HomePage : Page
{
  public HomePage()
  {
    this.InitializeComponent();
    this.Unloaded += HomePage_Unloaded;
  }

  private void HomePage_Unloaded(object sender, RoutedEventArgs e) => this.Bindings.StopTracking();

  private void Ex1_ColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
  {
    double ratio = AppColorHelper.GetContrastRatio(Ex1_BackgroundColorPicker.Color, Ex1_ForegroundColorPicker.Color);
    Ex1_ContrastTestTextBlock.Text = $"{Math.Round(ratio, 2)}";
    Ex1_RegularTestTextBlock.Text = (ratio >= 4.5) ? "Pass" : "Fail";
    Ex1_RegularTestTextBlock.Foreground = (ratio >= 4.5) ? new SolidColorBrush(Colors.DarkGreen) : new SolidColorBrush(Colors.DarkRed);
    Ex1_LargeTestTextBlock.Text = (ratio >= 3.0) ? "Pass" : "Fail";
    Ex1_LargeTestTextBlock.Foreground = (ratio >= 3.0) ? new SolidColorBrush(Colors.DarkGreen) : new SolidColorBrush(Colors.DarkRed);
    double luminance = AppColorHelper.GetRelativeLuminance(Ex1_BackdropColorPicker.Color);
    Ex1_LuminanceTestTextBlock.Text = $"{Math.Round(luminance, 2)}";
  }

  private void Ex2_ColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
  {
    var background = Ex2_BackgroundColorPicker.Color;
    var foreground = Ex2_ForegroundColorPicker.Color;
    var backLum = AppColorHelper.GetRelativeLuminance(background);
    var emphasis = AppColorHelper.GetComplementary(background);

    var backHsv = ToolkitColorHelper.ToHsv(background);
    var foreHsv = ToolkitColorHelper.ToHsv(foreground);
    Color emphasis2;
    var h = (backHsv.H - 180.0) / 360.0;

    if (foreHsv.V >= 0.5)
      emphasis2 = ToolkitColorHelper.FromHsv((h - Math.Floor(h)) * 360.0, Math.Max(backHsv.S, 0.5), Math.Clamp(backHsv.V, 0.5, 0.8), 1.0);
    else
      emphasis2 = ToolkitColorHelper.FromHsv((h - Math.Floor(h)) * 360.0, Math.Min(backHsv.S, 0.5), Math.Max(0.7, backHsv.V), 1.0);
    var contrast1 = AppColorHelper.GetContrastRatio(background, emphasis);
    var contrast2 = AppColorHelper.GetContrastRatio(Colors.Black, emphasis);


    Ex2_EmphasisColorPicker.Color = emphasis2;

    Ex2_TextBlock.Text = $"backLum: {Math.Round(backLum, 2)}\r\ncontrast1: {Math.Round(contrast1, 2)}\r\ncontrast2: {Math.Round(contrast2, 2)}";
  }

  private void Ex2_EmphasisColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
  {
    //var background = Ex2_BackgroundColorPicker.Color;
    //var backLum = AppColorHelper.GetRelativeLuminance(background);
    //var emphasis = args.NewColor;
    //var contrast1 = AppColorHelper.GetContrastRatio(background, emphasis);
    //var contrast2 = AppColorHelper.GetContrastRatio(Colors.Black, emphasis);
    Ex2_EmphasisBorder.Background = new SolidColorBrush(args.NewColor);
    //Ex2_TextBlock.Text = $"backLum: {Math.Round(backLum, 2)}\r\ncontrast1: {Math.Round(contrast1, 2)}\r\ncontrast2: {Math.Round(contrast2, 2)}";
  }
}
