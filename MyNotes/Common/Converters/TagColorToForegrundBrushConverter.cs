using MyNotes.Core.Model;

using ToolkitHelper = CommunityToolkit.WinUI.Helpers.ColorHelper;

namespace MyNotes.Common.Converters;

internal class TagColorToForegroundBrushConverter : IValueConverter
{
  static readonly Dictionary<TagColor, SolidColorBrush> _pairs = new()
  {
    {TagColor.Transparent, new SolidColorBrush(ToolkitHelper.ToColor("#E4000000"))},
    {TagColor.Red,  new SolidColorBrush(ToolkitHelper.ToColor("#FFD60000"))},
    {TagColor.Orange,  new SolidColorBrush(ToolkitHelper.ToColor("#FFE68A00"))},
    {TagColor.Yellow,  new SolidColorBrush(ToolkitHelper.ToColor("#FFB39500"))},
    {TagColor.Green,  new SolidColorBrush(ToolkitHelper.ToColor("#FF50991F"))},
    {TagColor.Mint,  new SolidColorBrush(ToolkitHelper.ToColor("#FF338059"))},
    {TagColor.Aqua,  new SolidColorBrush(ToolkitHelper.ToColor("#FF009999"))},
    {TagColor.Blue,  new SolidColorBrush(ToolkitHelper.ToColor("#FF0047D6"))},
    {TagColor.Violet,  new SolidColorBrush(ToolkitHelper.ToColor("#FF8F00D6"))},
  };

  public static SolidColorBrush Convert(object value)
    => value is TagColor tagColor && _pairs.TryGetValue(tagColor, out var brush) ? brush : _pairs[TagColor.Transparent];


  public object Convert(object value, Type targetType, object parameter, string language)
    => Convert(value);

  public object ConvertBack(object value, Type targetType, object parameter, string language)
    => throw new NotImplementedException();
}