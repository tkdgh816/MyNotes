using MyNotes.Core.Models;
using MyNotes.Core.Services;

namespace MyNotes.Core.ViewModels;

public class MainViewModel : ViewModelBase
{
  public MainViewModel(DatabaseService databaseService, NavigationService navigationService)
  {
    DatabaseService = databaseService;
    NavigationService = navigationService;

    NavigationGroupItem group1 = new("group 1", new BitmapIconSource() { UriSource = new Uri("ms-appx:///Assets/Emojis/0.png"), ShowAsMonochrome = false }, new Guid());
    group1.Children.Add(new NavigationListItem("list 1", new BitmapIconSource() { UriSource = new Uri("ms-appx:///Assets/Emojis/0.png"), ShowAsMonochrome = false }, new Guid()));
    group1.Children.Add(new NavigationListItem("list 2", new BitmapIconSource() { UriSource = new Uri("ms-appx:///Assets/Emojis/0.png"), ShowAsMonochrome = false }, new Guid()));
    group1.Children.Add(new NavigationListItem("list 3", new SymbolIconSource() { Symbol = Symbol.List }, new Guid()));

    NavigationGroupItem group2 = new("group 2", new SymbolIconSource() { Symbol = Symbol.List }, new Guid());
    group2.Children.Add(new NavigationListItem("list 1", new SymbolIconSource() { Symbol = Symbol.List }, new Guid()));
    group2.Children.Add(new NavigationListItem("list 2", new SymbolIconSource() { Symbol = Symbol.List }, new Guid()));
    group2.Children.Add(new NavigationListItem("list 3", new SymbolIconSource() { Symbol = Symbol.List }, new Guid()));

    NavigationGroupItem group3 = new("group 3", new SymbolIconSource() { Symbol = Symbol.List }, new Guid());
    group3.Children.Add(new NavigationListItem("list 1", new SymbolIconSource() { Symbol = Symbol.List }, new Guid()));
    group3.Children.Add(new NavigationListItem("list 2", new SymbolIconSource() { Symbol = Symbol.List }, new Guid()));
    group3.Children.Add(new NavigationListItem("list 3", new SymbolIconSource() { Symbol = Symbol.List }, new Guid()));
    group1.Children.Add(group3);

    ListMenuItemsGroup.Add(group1);
    ListMenuItemsGroup.Add(group2);

    MenuItems.Source = new List<IList>() { CoreMenuItemsGroup, ListMenuItemsGroup };
  }

  public DatabaseService DatabaseService { get; }
  public NavigationService NavigationService { get; }
  public CollectionViewSource MenuItems { get; } = new() { IsSourceGrouped = true };

  public List<Navigation> CoreMenuItemsGroup { get; } =
  [
    new NavigationHomeItem(),
    new NavigationBookmarksItem(),
    new NavigationTagsItem(),
    new NavigationSeparator(),
  ];
  public ObservableCollection<Navigation> ListMenuItemsGroup { get; } = new();
  public List<Navigation> FooterMenuItemsGroup { get; } =
  [
    new NavigationTrashItem(),
    new NavigationSettingsItem()
  ];
  public Stack<NavigationItem> PreviousNavigations { get; } = new();
  public NavigationItem? CurrentNavigation { get; set; }

  private bool _isBackEnabled = false;
  public bool IsBackEnabled
  {
    get => _isBackEnabled;
    set => SetProperty(ref _isBackEnabled, value);
  }
  public bool IsBackRequested = false;

}