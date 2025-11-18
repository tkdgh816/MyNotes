using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Controls.AnimatedVisuals;

using MyNotes.Strings;
using MyNotes.Views.Navigations;

namespace MyNotes.Models;

public interface INavigation { }

public sealed class NavigationSeparator : INavigation { }

public interface INavigationNode : INavigation { }

#region Core Nodes
public interface INavigationCoreNode : INavigationNode { }

public abstract class NavigationCoreNode : ObservableObject, INavigationCoreNode
{
  public required IconElement Icon
  {
    get;
    set => SetProperty(ref field, value);
  }

  public required string Title
  {
    get;
    set => SetProperty(ref field, value);
  }

  public required Type PageType
  {
    get;
    set => SetProperty(ref field, value);
  }
}

public sealed class NavigationHome : NavigationCoreNode
{
  public static NavigationHome Instance => field ??= new()
  {
    Icon = new IconSourceElement() { IconSource = new SymbolIconSource() { Symbol = Symbol.Home } },
    Title = Resources.NavigationHomeTitle,
    PageType = typeof(HomePage)
  };

  private NavigationHome() { }
}

public sealed class NavigationBookmarks : NavigationCoreNode
{
  public static NavigationBookmarks Instance => field ??= new()
  {
    Icon = new IconSourceElement() { IconSource = new SymbolIconSource() { Symbol = Symbol.Bookmarks } },
    Title = Resources.NavigationBookmarksTitle,
    PageType = typeof(HomePage)
  };

  private NavigationBookmarks() { }
}

public sealed class NavigationTrash : NavigationCoreNode
{
  public static NavigationTrash Instance => field ??= new()
  {
    Icon = new IconSourceElement() { IconSource = new SymbolIconSource() { Symbol = Symbol.Delete } },
    Title = Resources.NavigationTrashTitle,
    PageType = typeof(HomePage)
  };

  private NavigationTrash() { }
}

public sealed class NavigationSettings : NavigationCoreNode
{
  public static NavigationSettings Instance => field ??= new()
  {
    Icon = new AnimatedIcon() { Source = new AnimatedSettingsVisualSource() },
    Title = Resources.NavigationSettingsTitle,
    PageType = typeof(SettingsPage)
  };

  private NavigationSettings() { }
}
#endregion

#region User Nodes
public interface INavigationUserNode : INavigationNode { }

public class NavigationUserNode : ObservableObject, INavigationUserNode
{

}

public class NavigationUserCompositeNode : NavigationUserNode
{

}

public class NavigationUserLeafNode : NavigationUserNode
{

}

public sealed class NavigationUserRootNode : INavigationUserNode
{

}
#endregion