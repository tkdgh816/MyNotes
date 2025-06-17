using MyNotes.Core.Model;

using ToolkitColorHelper = CommunityToolkit.WinUI.Helpers.ColorHelper;

namespace MyNotes.Common.Converters;

internal class TagColorToBackgroundBrushConverter : IValueConverter
{
  static readonly Dictionary<TagColor, SolidColorBrush> _pairs = new()
  {
    { TagColor.Transparent, new SolidColorBrush(ToolkitColorHelper.ToColor("#09000000")) },
    { TagColor.Red,  new SolidColorBrush(ToolkitColorHelper.ToColor("#FFFFEBEB")) },
    { TagColor.Orange,  new SolidColorBrush(ToolkitColorHelper.ToColor("#FFFFF3E0")) },
    { TagColor.Yellow,  new SolidColorBrush(ToolkitColorHelper.ToColor("#FFFFFBE6")) },
    { TagColor.Green,  new SolidColorBrush(ToolkitColorHelper.ToColor("#FFEDFFE0")) },
    { TagColor.Mint,  new SolidColorBrush(ToolkitColorHelper.ToColor("#FFE0FFF0")) },
    { TagColor.Aqua,  new SolidColorBrush(ToolkitColorHelper.ToColor("#FFE0FFFF")) },
    { TagColor.Blue,  new SolidColorBrush(ToolkitColorHelper.ToColor("#FFE0EBFF")) },
    { TagColor.Violet,  new SolidColorBrush(ToolkitColorHelper.ToColor("#FFF8EBFF")) },
  };

  public static SolidColorBrush Convert(object value)
    => value is TagColor tagColor && _pairs.TryGetValue(tagColor, out var brush) ? brush : _pairs[TagColor.Transparent];

  public object Convert(object value, Type targetType, object parameter, string language)
    => Convert(value);

  public object ConvertBack(object value, Type targetType, object parameter, string language)
    => throw new NotImplementedException();
}