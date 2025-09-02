using ToolkitColorHelper = CommunityToolkit.WinUI.Helpers.ColorHelper;

namespace MyNotes.Debugging;
internal class ColorGenerator
{
  private static readonly List<Color> colors =
      [
        ToolkitColorHelper.ToColor("#FFFAF7A3"),
        ToolkitColorHelper.ToColor("#FFD5FACC"),
        ToolkitColorHelper.ToColor("#FF9CFADB"),
        ToolkitColorHelper.ToColor("#FFFFC4D1"),
        ToolkitColorHelper.ToColor("#FFEEDBFA"),
        ToolkitColorHelper.ToColor("#FFA5E5FA"),
        ToolkitColorHelper.ToColor("#FF93BDFF"),
        ToolkitColorHelper.ToColor("#FFFFB988"),
        ToolkitColorHelper.ToColor("#FFF7F7EC"),
        ToolkitColorHelper.ToColor("#FFC1C4C1"),
        ToolkitColorHelper.ToColor("#FFDBE6FA"),
        ToolkitColorHelper.ToColor("#FFEBDABA"),
        ToolkitColorHelper.ToColor("#FFEFFA87")
      ];

  public static Color GenerateColor()
  {
    Random random = new();
    return colors[random.Next(colors.Count)];
  }
}
