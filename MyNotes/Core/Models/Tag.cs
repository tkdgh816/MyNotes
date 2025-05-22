using MyNotes.Common.Collections;

namespace MyNotes.Core.Models;

public class Tags : SortedObervableCollection<string>, IComparable<Tags>
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

public class TagGroup : SortedObervableCollection<Tags>
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
}

#region Tags
//["Apple", "Apricot", "Banana", "Blackberry", "Blueberry", "Cherry",
//"Grapefruit", "Grapes", "Kiwi", "Mango", "Peach", "Melon",
//"Bear", "Dog", "Cat", "Goat", "Camel", "Donkey", "Horse", "Cow",
//"Rabbit", "Ant", "Elephant", "Mouse", "Whale", "Dolphin", "Eagle",
//"Parrot", "Swan", "Owl", "Chicken", "Lizard", "Frog", "Snake",]
#endregion