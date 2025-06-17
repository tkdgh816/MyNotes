using MyNotes.Common.Collections;

namespace MyNotes.Core.Model;

internal class Tag : ObservableObject, IEquatable<Tag>, IComparable<Tag>
{
  public TagId Id { get; }

  private string _text;
  public string Text
  {
    get => _text;
    set => SetProperty(ref _text, value);
  }

  private TagColor _color;
  public TagColor Color
  {
    get => _color;
    set => SetProperty(ref _color, value);
  }

  public Tag(TagId id, string text, TagColor color)
  {
    Id = id;
    _text = text;
    _color = color;
  }

  public bool Equals(Tag? other) => other is not null && other.Text == Text;
  public int CompareTo(Tag? other) => other is null ? 1 : Text.CompareTo(other.Text);
}

internal enum TagColor
{
  // Color           Background Foreground
  Transparent,    // #09000000, #E4000000
  Red,            // #FFFFEBEB, #FFD60000 
  Orange,         // #FFFFF3E0, #FFE68A00
  Yellow,         // #FFFFFBE6, #FFB39500
  Green,          // #FFEDFFE0, #FF50991F
  Mint,           // #FFE0FFF0, #FF338059
  Aqua,           // #FFE0FFFF, #FF009999
  Blue,           // #FFE0EBFF, #FF0047D6
  Violet,         // #FFF8EBFF, #FF8F00D6
}

internal class Tags : SortedObservableCollection<Tag>, IComparable<Tags>
{
  public string Key { get; }

  public Tags(string key) : base() => Key = key;
  public Tags(string key, IEnumerable<Tag> items) : base(items) => Key = key;

  public int CompareTo(Tags? other) => other is null ? 1 : Key.CompareTo(other.Key);
}

internal class TagGroup : SortedObservableCollection<Tags>
{
  public void AddItem(Tag tag)
  {
    string key = tag.Text[0].ToString();
    Tags? tags = FindGroup(key);
    if (tags is null)
      Add(new Tags(key, [tag]));
    else
      tags.Add(tag);
  }

  public bool RemoveItem(Tag tag)
  {
    string key = tag.Text[0].ToString();
    Tags? tags = FindGroup(key);
    return tags is not null && tags.Remove(tag);
  }

  public Tags? FindGroup(string key)
  {
    foreach (Tags tags in this)
      if (tags.Key == key)
        return tags;
    return null;
  }

  public bool Contains(Tag tag)
  {
    string key = tag.Text[0].ToString();
    Tags? tags = FindGroup(key);
    return tags is not null && tags.Contains(tag);
  }

  public bool Contains(string tagText)
  {
    string key = tagText[0].ToString();
    Tags? tags = FindGroup(key);
    return tags is not null && tags.Any(tag => tag.Text == tagText);
  }

  public IEnumerable<Tag> GetTagsAll()
  {
    foreach (Tags tags in this)
      foreach (Tag tag in tags)
        yield return tag;
  }

  public IEnumerable<Tag> FindTags(string text)
  {
    if (string.IsNullOrEmpty(text))
      yield break;

    foreach (Tags tags in this)
      foreach (Tag tag in tags.Where(t => t.Text.Contains(text, StringComparison.CurrentCultureIgnoreCase)))
        yield return tag;
  }
}