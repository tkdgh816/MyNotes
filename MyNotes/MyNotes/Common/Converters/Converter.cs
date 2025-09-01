using MyNotes.Core.Model;

namespace MyNotes.Common.Converters;

internal static class Converter
{
  public static IconSource? IconToIconSourceConverter(Icon icon)
  {
    if (icon is Glyph glyph)
      return new FontIconSource() { Glyph = glyph.Code };
    else if (icon is Emoji emoji)
      return new BitmapIconSource() { UriSource = new Uri(emoji.Path), ShowAsMonochrome = false };
    else
      return null;
  }
}
