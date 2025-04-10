using MyNotes.Core.Views;

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

public class NavigationSearchItem
{ }

public class NavigationGroupItem : NavigationItem
{
  public Guid Id { get; }
  public ObservableCollection<NavigationItem> Children { get; } = new();

  public NavigationGroupItem(string name, IconSource icon, Guid id) : base(typeof(ListsGroupPage), name, icon) 
  {
    Id = id;
  }
}

public class NavigationListItem : NavigationItem
{
  public Guid Id { get; }

  public NavigationListItem(string name, IconSource icon, Guid id) : base(typeof(NotesListPage), name, icon) 
  {
    Id = id;
  }
}