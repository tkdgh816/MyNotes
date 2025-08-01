using MyNotes.Common.Collections;
using MyNotes.Core.View;

namespace MyNotes.Core.Model;

internal abstract class Navigation : ObservableObject
{
  private bool _isEditable = false;
  public bool IsEditable
  {
    get => _isEditable;
    set => SetProperty(ref _isEditable, value);
  }
}

internal class NavigationSeparator : Navigation
{ }

internal abstract class NavigationItem : Navigation
{
  protected NavigationItem(Type pageType, string name, Icon icon)
  {
    PageType = pageType;
    Name = name;
    Icon = icon;
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

internal abstract class NavigationBoard : NavigationItem
{
  protected NavigationBoard(Type pageType, string name, Icon icon) : base(pageType, name, icon) { }
}

internal class NavigationSearch : NavigationBoard
{
  public string SearchText { get; }

  public NavigationSearch(string searchText) : base(typeof(BoardPage), $"Search results for \"{searchText}\"", IconManager.FindGlyph("\uE721"))
  {
    SearchText = searchText;
  }
}

internal class NavigationHome : NavigationItem
{
  public NavigationHome() : base(typeof(HomePage), "Home", IconManager.FindGlyph("\uE80F")) { }
}

internal class NavigationTags : NavigationBoard
{
  public NavigationTags() : base(typeof(BoardPage), "Tags", IconManager.FindGlyph("\uE8EC")) { }
}

internal class NavigationTrash : NavigationBoard
{
  public NavigationTrash() : base(typeof(BoardPage), "Trash", IconManager.FindGlyph("\uE74D")) { }
}

internal class NavigationSettings : NavigationItem
{
  public NavigationSettings() : base(typeof(SettingsPage), "Settings", IconManager.FindGlyph("\uE713")) { }
}

internal class NavigationBookmarks : NavigationBoard
{
  public NavigationBookmarks() : base(typeof(BoardPage), "Bookmarks", IconManager.FindGlyph("\uE734")) { }
}

internal class NavigationUserBoard : NavigationBoard
{
  public BoardId Id { get; }
  public NavigationUserGroup? Parent { get; set; }

  public NavigationUserBoard(string name, Icon icon, BoardId id) : base(typeof(BoardPage), name, icon) => Id = id;

  public NavigationUserBoard? GetNext()
  {
    if (Parent is null)
      return null;
    int index = Parent.IndexOfChild(this);
    return index == Parent.ChildCount - 1 ? null : Parent.GetChild(index + 1);
  }

  public NavigationUserBoard? GetPrevious()
  {
    if (Parent is null)
      return null;
    int index = Parent.IndexOfChild(this);
    return index <= 0 ? null : Parent.GetChild(index - 1);
  }
}

internal class NavigationUserGroup : NavigationUserBoard
{
  private readonly ObservableCollection<NavigationUserBoard> _children = new();
  public ReadOnlyObservableCollection<NavigationUserBoard> Children { get; }
  public event NotifyCollectionChangedEventHandler? ChildrenChanged;

  public NavigationUserGroup(string name, Icon icon, BoardId id) : base(name, icon, id)
  {
    PageType = typeof(UserGroupPage);
    Children = new(_children);
    _children.CollectionChanged += OnChildrenCollectionChanged;
  }

  private void OnChildrenCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    => ChildrenChanged?.Invoke(this, e);

  public void AddChild(NavigationUserBoard item)
  {
    item.Parent?.RemoveChild(item);
    item.Parent = this;
    _children.Add(item);
  }

  public void AddChildren(params NavigationUserBoard[] items)
  {
    foreach (NavigationUserBoard item in items)
      AddChild(item);
  }

  public void InsertChild(int index, NavigationUserBoard item)
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
      item.Parent?.RemoveChild(item);
      item.Parent = this;
      _children.Insert(index, item);
    }
  }

  public bool RemoveChild(NavigationUserBoard item) => _children.Contains(item) && _children.Remove(item);

  public void RemoveChildAt(int index) => _children.RemoveAt(index);

  public int IndexOfChild(NavigationUserBoard item) => _children.IndexOf(item);

  public NavigationUserBoard GetChild(int index) => _children[index];

  public bool ContainsChild(NavigationUserBoard item) => _children.Contains(item);

  public int ChildCount => _children.Count;
}

internal class NavigationUserRootGroup : NavigationUserGroup
{
  public NavigationUserRootGroup() : base("Root", IconManager.FindGlyph("\uE82D"), BoardId.Empty)
  { }
}

internal enum TreeTraversalOrder { Pre, Post }

internal enum BoardViewStyle
{
  // Grid
  Grid_160_0, Grid_160_50, Grid_160_100,
  Grid_200_0, Grid_200_50, Grid_200_100,
  Grid_240_0, Grid_240_50, Grid_240_100,
  Grid_280_0, Grid_280_50, Grid_280_100,
  Grid_320_0, Grid_320_50, Grid_320_100,
  // List
  List_160, List_200, List_240, List_280, List_320
}