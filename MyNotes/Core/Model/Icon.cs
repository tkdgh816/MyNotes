namespace MyNotes.Core.Model;

internal enum IconType { Glyph, Emoji }

internal abstract record Icon
{
  public IconType IconType { get; init; }
  public string Code { get; init; }
  public string Description { get; init; }

  protected Icon(IconType iconType, string code, string description)
  {
    IconType = iconType;
    Code = code;
    Description = description;
  }
}

internal record Glyph : Icon
{
  public Glyph() : this("\uE76E", "Emoji2") { }

  public Glyph(string code, string description) : base(IconType.Glyph, code, $"{description}, &#x{code.Select(c => char.ConvertToUtf32(code, code.IndexOf(c))).First():X};") { }
}

internal record Emoji : Icon
{
  public int Index { get; }
  public string Path { get; }

  public Emoji() : this("\uD83D\uDE00", "grinning face", 0) { }

  public Emoji(string code, string description, int index) : base(IconType.Emoji, code, description)
  {
    Index = index;
    Path = $"ms-appx:///Assets/icons/emojis/{index}.png";
  }
}