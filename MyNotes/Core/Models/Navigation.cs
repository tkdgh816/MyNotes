using MyNotes.Core.Views;

namespace MyNotes.Core.Models;

public abstract class Navigation : ObservableObject
{ }

public class NavigationSeparator : Navigation
{ }

public abstract class NavigationItem(Type pageType, string name, IconSource icon) : Navigation
{
  public Type PageType { get; protected set; } = pageType;

  private string _name = name;
  public string Name
  {
    get => _name;
    set => SetProperty(ref _name, value);
  }

  private IconSource _icon = icon;
  public IconSource Icon
  {
    get => _icon;
    set => SetProperty(ref _icon, value);
  }
}

public class NavigationSearchItem : NavigationItem
{
  public NavigationSearchItem(string searchText) : base(typeof(SearchPage), searchText, new SymbolIconSource() { Symbol = Symbol.Find }) { }
}

public class NavigationHomeItem : NavigationItem
{
  public NavigationHomeItem() : base(typeof(HomePage), "Home", new SymbolIconSource() { Symbol = Symbol.Home }) { }
}

public class NavigationBookmarksItem : NavigationItem
{
  public NavigationBookmarksItem() : base(typeof(BookmarksPage), "Bookmarks", new SymbolIconSource() { Symbol = Symbol.Favorite }) { }
}

public class NavigationTagsItem : NavigationItem
{
  public NavigationTagsItem() : base(typeof(TagsPage), "Tags", new SymbolIconSource() { Symbol = Symbol.Tag }) { }
}

public class NavigationTrashItem : NavigationItem
{
  public NavigationTrashItem() : base(typeof(TrashPage), "Trash", new SymbolIconSource() { Symbol = Symbol.Delete }) { }
}

public class NavigationSettingsItem : NavigationItem
{
  public NavigationSettingsItem() : base(typeof(SettingsPage), "Settings", new SymbolIconSource() { Symbol = Symbol.Setting }) { }
}

public class NavigationBoardItem : NavigationItem
{
  public Guid Id { get; }
  public NavigationBoardGroupItem Parent { get; set; } = null!;

  public NavigationBoardItem(string name, IconSource icon, Guid id) : base(typeof(NotesListPage), name, icon) 
  {
    Id = id;
  }

  private bool _isEditMode = false;
  public bool IsEditMode
  {
    get => _isEditMode;
    set => SetProperty(ref _isEditMode, value);
  }
}

public class NavigationBoardGroupItem : NavigationBoardItem
{
  private readonly ObservableCollection<NavigationBoardItem> _children = new();
  public ReadOnlyObservableCollection<NavigationBoardItem> Children { get; }

  public NavigationBoardGroupItem(string name, IconSource icon, Guid id) : base(name, icon, id)
  {
    PageType = typeof(ListsGroupPage);
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