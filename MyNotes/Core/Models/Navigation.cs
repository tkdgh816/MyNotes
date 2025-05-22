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

public class NavigationSearchItem : NavigationItem
{
  public NavigationSearchItem(string searchText) : base(typeof(SearchPage), searchText, IconLibrary.FindGlyph("\uE721")) { }
}

public class NavigationHomeItem : NavigationItem
{
  public NavigationHomeItem() : base(typeof(HomePage), "Home", IconLibrary.FindGlyph("\uE80F")) { }
}

public class NavigationBookmarksItem : NavigationItem
{
  public NavigationBookmarksItem() : base(typeof(BookmarksPage), "Bookmarks", IconLibrary.FindGlyph("\uE734")) { }
}

public class NavigationTagsItem : NavigationItem
{
  public NavigationTagsItem() : base(typeof(TagsPage), "Tags", IconLibrary.FindGlyph("\uE8EC"))
  {
    SelectsOnInvoked = false;
  }
}

public class NavigationTrashItem : NavigationItem
{
  public NavigationTrashItem() : base(typeof(TrashPage), "Trash", IconLibrary.FindGlyph("\uE74D")) { }
}

public class NavigationSettingsItem : NavigationItem
{
  public NavigationSettingsItem() : base(typeof(SettingsPage), "Settings", IconLibrary.FindGlyph("\uE713")) { }
}

public class NavigationBoardItem : NavigationItem
{
  public Guid Id { get; }
  public NavigationBoardGroupItem? Parent { get; set; }
  public ObservableCollection<Note> Notes { get; } = new();

  public NavigationBoardItem(string name, Icon icon, Guid id) : base(typeof(NoteBoardPage), name, icon)
  {
    Id = id;
  }

  private async Task<Note[]> LoadItemsAsync(uint offset)
  {
    await Task.Delay(500);
    List<Note> items = new();
    return items.ToArray();
  }

  public NavigationBoardItem? GetNext()
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

public class NavigationBoardGroupItem : NavigationBoardItem
{
  private readonly ObservableCollection<NavigationBoardItem> _children = new();
  public ReadOnlyObservableCollection<NavigationBoardItem> Children { get; }

  public NavigationBoardGroupItem(string name, Icon icon, Guid id) : base(name, icon, id)
  {
    PageType = typeof(NoteBoardGroupPage);
    Children = new(_children);
  }

  public void Add(params NavigationBoardItem[] items)
  {
    foreach (NavigationBoardItem item in items)
    {
      item.Parent?.Remove(item);
      item.Parent = this;
      _children.Add(item);
    }
  }

  public void Insert(int index, NavigationBoardItem item)
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

  public bool Remove(NavigationBoardItem item) => _children.Contains(item) && _children.Remove(item);

  public void RemoveAt(int index) => _children.RemoveAt(index);
}

public class NavigationBoardGroupRootItem : NavigationBoardGroupItem
{
  public NavigationBoardGroupRootItem() : base("Root", IconLibrary.FindGlyph("\uE82D"), Guid.Empty)
  { }
}