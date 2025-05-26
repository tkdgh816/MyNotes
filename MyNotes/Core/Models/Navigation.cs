using MyNotes.Common.Collections;
using MyNotes.Core.Views;

namespace MyNotes.Core.Models;

public abstract class Navigation : ObservableObject
{
  private bool _isEditable = false;
  public bool IsEditable
  {
    get => _isEditable;
    set => SetProperty(ref _isEditable, value);
  }

  private bool _selectsOnInvoked = true;
  public bool SelectsOnInvoked
  {
    get => _selectsOnInvoked;
    set => SetProperty(ref _selectsOnInvoked, value);
  }
}

public class NavigationSeparator : Navigation
{ }

public abstract class NavigationItem : Navigation
{
  protected NavigationItem(Type pageType, string name, Icon icon)
  {
    PageType = pageType;
    Name = name;
    Icon = icon;
    PropertyChanged += (s, e) =>
    {
      Debug.WriteLine($"{Name}: {e.PropertyName}");
    };
  }

  public Type PageType { get; protected set; }

  private string _name = null!;
  public string Name
  {
    get => _name;
    set => SetProperty(ref _name, value);
  }

  private Icon _icon = null!;
  public Icon Icon
  {
    get => _icon;
    set => SetProperty(ref _icon, value);
  }
}

public abstract class NavigationNotes : NavigationItem
{
  public SortedObervableCollection<Note> Notes { get; set; } = new();

  protected NavigationNotes(Type pageType, string name, Icon icon) : base(pageType, name, icon) { }
}

public class NavigationSearch : NavigationNotes
{
  public NavigationSearch(string searchText) : base(typeof(SearchPage), searchText, IconLibrary.FindGlyph("\uE721")) { }
}

public class NavigationHome : NavigationItem
{
  public NavigationHome() : base(typeof(HomePage), "Home", IconLibrary.FindGlyph("\uE80F")) { }
}

public class NavigationTags : NavigationNotes
{
  public NavigationTags() : base(typeof(TagsPage), "Tags", IconLibrary.FindGlyph("\uE8EC"))
  {
    SelectsOnInvoked = false;
  }
}

public class NavigationTrash : NavigationItem
{
  public NavigationTrash() : base(typeof(TrashPage), "Trash", IconLibrary.FindGlyph("\uE74D")) { }
}

public class NavigationSettings : NavigationItem
{
  public NavigationSettings() : base(typeof(SettingsPage), "Settings", IconLibrary.FindGlyph("\uE713")) { }
}

public class NavigationBookmarks : NavigationNotes
{
  public NavigationBookmarks() : base(typeof(NoteHubPage), "Bookmarks", IconLibrary.FindGlyph("\uE734")) { }
}

public class NavigationBoard : NavigationNotes
{
  public Guid Id { get; }
  public NavigationBoardGroup? Parent { get; set; }

  public NavigationBoard(string name, Icon icon, Guid id) : base(typeof(NoteHubPage), name, icon)
  {
    Id = id;
  }

  private async Task<Note[]> LoadItemsAsync(uint offset)
  {
    await Task.Delay(500);
    List<Note> items = new();
    return items.ToArray();
  }

  public NavigationBoard? GetNext()
  {
    if (Parent is null)
      return null;
    int index = Parent.Children.IndexOf(this);
    if (index == Parent.Children.Count - 1)
      return null;
    else
      return Parent.Children[index + 1];
  }
}

public class NavigationBoardGroup : NavigationBoard
{
  private readonly ObservableCollection<NavigationBoard> _children = new();
  public ReadOnlyObservableCollection<NavigationBoard> Children { get; }

  public NavigationBoardGroup(string name, Icon icon, Guid id) : base(name, icon, id)
  {
    PageType = typeof(NoteBoardGroupPage);
    Children = new(_children);
  }

  public void Add(params NavigationBoard[] items)
  {
    foreach (NavigationBoard item in items)
    {
      item.Parent?.Remove(item);
      item.Parent = this;
      _children.Add(item);
    }
  }

  public void Insert(int index, NavigationBoard item)
  {
    if (item.Parent == this)
    {
      int oldIndex = _children.IndexOf(item);
      if (oldIndex != index)
      {
        _children.RemoveAt(oldIndex);
        _children.Insert((oldIndex > index) ? index : index - 1, item);
      }
    }
    else
    {
      item.Parent?.Remove(item);
      item.Parent = this;
      _children.Insert(index, item);
    }
  }

  public bool Remove(NavigationBoard item) => _children.Contains(item) && _children.Remove(item);

  public void RemoveAt(int index) => _children.RemoveAt(index);
}

public class NavigationBoardRootGroup : NavigationBoardGroup
{
  public NavigationBoardRootGroup() : base("Root", IconLibrary.FindGlyph("\uE82D"), Guid.Empty)
  { }
}