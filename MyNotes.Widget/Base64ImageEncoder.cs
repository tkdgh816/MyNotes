using System;
using System.Text;

using Windows.UI;

namespace MyNotes.Widget;

public static class Base64ImageEncoder
{
  public static string CreateSolidColorSVG(Color color, int width = 100, int height = 100)
  {
    StringBuilder sb = new();
    string opacity = color.A == 255 ? "" : $"""fill-opacity="{color.A / 256.0f:F3}" """;
    sb.Append($"""<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" viewBox="0 0 {width} {height}">""");
    sb.Append($"""<rect width="100%" height="100%" fill="#{color.R:X2}{color.G:X2}{color.B:X2}" {opacity}/>""");
    sb.Append("""</svg>""");

    var base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString()));
    Console.WriteLine(sb.ToString());
    return $"data:image/svg+xml;base64,{base64String}";
  }
}