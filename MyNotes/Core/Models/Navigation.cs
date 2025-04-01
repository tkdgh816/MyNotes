namespace MyNotes.Core.Models;

public abstract class Navigation : ObservableObject
{ }

public class NavigationSeparator : Navigation
{ }

public abstract class NavigationItem(Type pageType, string name, IconSource icon) : Navigation
{
  public Type PageType { get; } = pageType;

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

public class NavigationHomeItem : NavigationItem
{
  public NavigationHomeItem() : base(null, "Home", new SymbolIconSource() { Symbol = Symbol.Home}) { }
}

public class NavigationBookmarksItem : NavigationItem
{
  public NavigationBookmarksItem() : base(null, "Bookmarks", new SymbolIconSource() { Symbol = Symbol.Favorite }) { }
}

public class NavigationTagsItem : NavigationItem
{
  public NavigationTagsItem() : base(null, "Tags", new SymbolIconSource() { Symbol = Symbol.Tag }) { }
}

public class NavigationTrashItem : NavigationItem
{
  public NavigationTrashItem() : base(null, "Trash", new SymbolIconSource() { Symbol = Symbol.Delete }) { }
}

public class NavigationGroupItem
{ }

public class NavigationListItem
{ }