using MyNotes.Common.Collections;

namespace MyNotes.Core.Model;

internal class Tags : SortedObservableCollection<string>, IComparable<Tags>
{
  public string Key { get; }

  public Tags(string key) => Key = key;
  public Tags(string key, IEnumerable<string> items) : base(items) => Key = key;

  public int CompareTo(Tags? other)
  {
    if (other is null)
      return 1;
    return Key.CompareTo(other.Key);
  }
}

internal class TagGroup : SortedObservableCollection<Tags>
{
  public void AddItem(string tag)
  {
    string key = tag[0].ToString();
    Tags? tags = FindGroup(key);
    if (tags is null)
      Add(new Tags(key, [tag]));
    else
      tags.Add(tag);
  }

  public bool RemoveItem(string tag)
  {
    string key = tag[0].ToString();
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

  public bool Contains(string tag)
  {
    string key = tag[0].ToString();
    Tags? tags = FindGroup(key);
    return tags is not null && tags.Contains(tag);
  }

  public IEnumerable<string> GetTagsAll()
  {
    foreach (Tags tags in this)
      foreach (string tag in tags)
        yield return tag;
  }

  public IEnumerable<string> FindTags(string text)
  {
    if (string.IsNullOrEmpty(text))
      yield break;

    foreach (Tags tags in this)
      foreach (string tag in tags.Where(t => t.Contains(text, StringComparison.CurrentCultureIgnoreCase)))
        yield return tag;
  }
}