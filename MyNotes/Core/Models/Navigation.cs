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
//important: typeof(Page) 부분은 각 메뉴의 상세 페이지로 교체할 것
public class NavigationHomeItem : NavigationItem
{
  public NavigationHomeItem() : base(typeof(Page), "Home", new SymbolIconSource() { Symbol = Symbol.Home}) { }
}

public class NavigationBookmarksItem : NavigationItem
{
  public NavigationBookmarksItem() : base(typeof(Page), "Bookmarks", new SymbolIconSource() { Symbol = Symbol.Favorite }) { }
}

public class NavigationTagsItem : NavigationItem
{
  public NavigationTagsItem() : base(typeof(Page), "Tags", new SymbolIconSource() { Symbol = Symbol.Tag }) { }
}

public class NavigationTrashItem : NavigationItem
{
  public NavigationTrashItem() : base(typeof(Page), "Trash", new SymbolIconSource() { Symbol = Symbol.Delete }) { }
}

public class NavigationGroupItem
{ }

public class NavigationListItem
{ }