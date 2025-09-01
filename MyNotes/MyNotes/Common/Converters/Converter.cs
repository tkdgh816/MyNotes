using MyNotes.Core.Model;

using ToolkitColorHelper = CommunityToolkit.WinUI.Helpers.ColorHelper;


namespace MyNotes.Common.Converters;

internal static class Converter
{
  public static IconSource? ToIconSource(Icon icon)
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

  public static SolidColorBrush ToTagBackground(TagColor color) => TagBackgroundPairs.TryGetValue(color, out var brush) ? brush : TagBackgroundPairs[TagColor.White];
  public static SolidColorBrush ToTagForeground(TagColor color) => TagForegroundPairs.TryGetValue(color, out var brush) ? brush : TagForegroundPairs[TagColor.White];

  public static BitmapIconSource IntersectIconSource = new() { UriSource = new Uri("/Assets/icons/fluent/FluentShapeIntersect24Regular.png") };
  public static BitmapIconSource UnionIconSource = new() { UriSource = new Uri("/Assets/icons/fluent/FluentShapeUnion24Regular.png") };

  public static BitmapIconSource ToSetOperationIconSource(bool boolValue) => boolValue ? IntersectIconSource : UnionIconSource;

  public static bool EmptyStringToBool(string? text) => string.IsNullOrEmpty(text);

  public static string Concat(string text, string prefix, string suffix) => string.Concat(prefix, text, prefix);
  public static string Join(IEnumerable values, string separator) => string.Join(separator, values.Cast<object>().Select(obj => obj.ToString()));

  public static bool NullToBool(object? obj) => obj is null;
  public static bool NotNullToBool(object? obj) => obj is not null;
}
