namespace MyNotes.Core.Models;

public enum IconType { Glyph, Emoji }

public abstract class Icon
{
  public IconType IconType { get; protected set; }
  public string Code { get; protected set; }
  public string? Description { get; set; }

  public Icon(string code)
  {
    Code = code;
  }

  public Icon(string code, string description)
  {
    Code = code;
    Description = description;
  }
}

public class Glyph : Icon
{
  public Glyph() : this("\uE76E", "Emoji2") { }

  public Glyph(string code) : base(code) 
  {
    IconType = IconType.Glyph;
  }

  public Glyph(string code, string description) : base(code, $"{description}, &#x{code.Select(c => char.ConvertToUtf32(code, code.IndexOf(c))).First():X};") 
  {
    IconType = IconType.Glyph;
  }
}

public class Emoji : Icon
{
  public int Index { get; }
  public string Path { get; }

  public Emoji() : this("\uD83D\uDE00", "grinning face", 0)
  { }

  public Emoji(string code, int index) : base(code)
  {
    IconType = IconType.Emoji;
    Index = index;
    Path = $"ms-appx:///Assets/icons/emojis/{index}.png";
  }

  public Emoji(string code, string description, int index) : base(code, description)
  {
    IconType = IconType.Emoji;
    Index = index;
    Path = $"ms-appx:///Assets/icons/emojis/{index}.png";
  }
}