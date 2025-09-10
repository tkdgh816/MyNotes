using MyNotes.Core.Model;

using ToolkitColorHelper = CommunityToolkit.WinUI.Helpers.ColorHelper;
using MyNotes.Core.Shared;

namespace MyNotes.Common.Helpers;

internal static class Converter
{
  public static IconSource? GetIconSource(Icon icon)
  {
    if (icon is Glyph glyph)
      return new FontIconSource() { Glyph = glyph.Code };
    else if (icon is Emoji emoji)
      return new BitmapIconSource() { UriSource = new Uri(emoji.Path), ShowAsMonochrome = false };
    else
      return null;
  }

  public static readonly Dictionary<TagColor, SolidColorBrush> TagBackgroundPairs = new()
  {
    { TagColor.White, new SolidColorBrush(ToolkitColorHelper.ToColor("#FFFFFFFF")) },
    { TagColor.Red,  new SolidColorBrush(ToolkitColorHelper.ToColor("#FFFFEBEB")) },
    { TagColor.Orange,  new SolidColorBrush(ToolkitColorHelper.ToColor("#FFFFF3E0")) },
    { TagColor.Yellow,  new SolidColorBrush(ToolkitColorHelper.ToColor("#FFFFFBE6")) },
    { TagColor.Green,  new SolidColorBrush(ToolkitColorHelper.ToColor("#FFEDFFE0")) },
    { TagColor.Mint,  new SolidColorBrush(ToolkitColorHelper.ToColor("#FFE0FFF0")) },
    { TagColor.Aqua,  new SolidColorBrush(ToolkitColorHelper.ToColor("#FFE0FFFF")) },
    { TagColor.Blue,  new SolidColorBrush(ToolkitColorHelper.ToColor("#FFE0EBFF")) },
    { TagColor.Violet,  new SolidColorBrush(ToolkitColorHelper.ToColor("#FFF8EBFF")) },
  };

  public static readonly Dictionary<TagColor, SolidColorBrush> TagForegroundPairs = new()
  {
    {TagColor.White, new SolidColorBrush(ToolkitColorHelper.ToColor("#FF202020"))},
    {TagColor.Red,  new SolidColorBrush(ToolkitColorHelper.ToColor("#FFD60000"))},
    {TagColor.Orange,  new SolidColorBrush(ToolkitColorHelper.ToColor("#FFE68A00"))},
    {TagColor.Yellow,  new SolidColorBrush(ToolkitColorHelper.ToColor("#FFB39500"))},
    {TagColor.Green,  new SolidColorBrush(ToolkitColorHelper.ToColor("#FF50991F"))},
    {TagColor.Mint,  new SolidColorBrush(ToolkitColorHelper.ToColor("#FF338059"))},
    {TagColor.Aqua,  new SolidColorBrush(ToolkitColorHelper.ToColor("#FF009999"))},
    {TagColor.Blue,  new SolidColorBrush(ToolkitColorHelper.ToColor("#FF0047D6"))},
    {TagColor.Violet,  new SolidColorBrush(ToolkitColorHelper.ToColor("#FF8F00D6"))},
  };

  public static SolidColorBrush GetTagBackgroundBrush(TagColor color) => TagBackgroundPairs.TryGetValue(color, out var brush) ? brush : TagBackgroundPairs[TagColor.White];
  public static SolidColorBrush GetTagForegroundBrush(TagColor color) => TagForegroundPairs.TryGetValue(color, out var brush) ? brush : TagForegroundPairs[TagColor.White];

  public static BitmapIconSource IntersectIconSource = new() { UriSource = new Uri("ms-appx:///Assets/icons/fluent/FluentShapeIntersect24Regular.png") };
  public static BitmapIconSource UnionIconSource = new() { UriSource = new Uri("ms-appx:///Assets/icons/fluent/FluentShapeUnion24Regular.png") };

  public static BitmapIconSource GetOperationIconSource(bool boolValue) => boolValue ? IntersectIconSource : UnionIconSource;

  public static bool IsEmptyString(string? text) => string.IsNullOrEmpty(text);
  public static bool IsValidString(string? text) => !string.IsNullOrEmpty(text);

  public static string WrapStringWith(string text, string prefix, string suffix) => string.Concat(prefix, text, prefix);
  public static string JoinStrings(IEnumerable values, string separator) => string.Join(separator, values.Cast<object>().Select(obj => obj.ToString()));

  public static bool IsNull(object? obj) => obj is null;
  public static bool IsNotNull(object? obj) => obj is not null;

  public static SolidColorBrush GetOverlayColorBrush(Color color, Color background) => new SolidColorBrush(ColorHelper.GetAlphaBlendingColor(color, background));

  private readonly static SolidColorBrush _brushLight = new(Microsoft.UI.ColorHelper.FromArgb(96, 255, 255, 255));
  private readonly static SolidColorBrush _brushMediumLight = new(Microsoft.UI.ColorHelper.FromArgb(96, 200, 200, 200));
  private readonly static SolidColorBrush _brushMediumDark = new(Microsoft.UI.ColorHelper.FromArgb(16, 16, 16, 16));
  private readonly static SolidColorBrush _brushDark = new(Microsoft.UI.ColorHelper.FromArgb(16, 0, 0, 0));
  public static SolidColorBrush GetLayerColorBrush(Color color) => (color.A < 64)
    ? _brushDark
    : ColorHelper.GetRelativeLuminance(color) switch
    {
      >= 0 and <= 0.25 => _brushLight,
      > 0.25 and <= 0.5 => _brushMediumLight,
      > 0.5 and <= 0.75 => _brushMediumDark,
      > 0.75 and <= 1.0 => _brushDark,
      _ => _brushDark
    };

  public static string GetConditionalString(bool boolValue, string trueValue, string falseValue) => boolValue ? trueValue : falseValue;

  public static bool NegateBool(bool boolValue) => !boolValue;
  public static Visibility GetVisibleIfFalse(bool boolValue) => boolValue ? Visibility.Collapsed : Visibility.Visible;

  public static SolidColorBrush GetColorBrush(Color color) => new SolidColorBrush(color);

  public static Visibility GetVisibleIfUserBoard(Navigation navigation) => navigation is NavigationUserBoard ? Visibility.Visible : Visibility.Collapsed;

  public static Visibility GetVisibleIfPositive(int number) => number > 0 ? Visibility.Visible : Visibility.Collapsed;
  public static Visibility GetVisibleIfNotPositive(int number) => number <= 0 ? Visibility.Visible : Visibility.Collapsed;

  public static int BackdropKindToInt(BackdropKind backdropKind) => (int)backdropKind;
  public static int AppThemeToInt(AppTheme theme) => (int)theme;
  public static int AppLanguageToInt(AppLanguage language) => (int)language;

  public static BackdropKind ToBackdropKind(int index) => (BackdropKind)index;
  public static AppTheme ToAppTheme(int index) => (AppTheme)index;
  public static AppLanguage ToAppLanguage(int index) => (AppLanguage)index;
}
