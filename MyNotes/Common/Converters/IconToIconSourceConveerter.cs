using MyNotes.Core.Model;

namespace MyNotes.Common.Converters;

internal class IconGetIconSourceConveerter : IValueConverter
{
  public static IconSource? Convert(object value)
  {
    if (value is Glyph glyph)
      return new FontIconSource() { Glyph = glyph.Code };
    else if (value is Emoji emoji)
      return new BitmapIconSource() { UriSource = new Uri(emoji.Path), ShowAsMonochrome = false };
    else
      return null;
  }

  public object? Convert(object value, Type targetType, object parameter, string language)
    => Convert(value);

  public object ConvertBack(object value, Type targetType, object parameter, string language)
    => throw new NotImplementedException();
}
