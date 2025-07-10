using Microsoft.Extensions.DependencyInjection;
using Microsoft.Windows.Globalization;

using MyNotes.Core.Dao;
using MyNotes.Core.Service;
using MyNotes.Core.ViewModel;

namespace MyNotes;

public partial class App : Application
{
  public App()
  {
    this.InitializeComponent();
  }

  protected override void OnLaunched(LaunchActivatedEventArgs args)
  {
    GetService<WindowService>().GetMainWindow().Activate();
    var settingsService = GetService<SettingsService>();
    Debug.WriteLine(string.Join(", ", ApplicationLanguages.Languages));
    Debug.WriteLine(string.Join(", ", ApplicationLanguages.ManifestLanguages));
  }

  public static App Instance => (App)Current;

  #region Services
  public ServiceProvider Services { get; } = ConfigureServices();
  private static ServiceProvider ConfigureServices()
  {
    ServiceCollection services = new();

    // Services
    services.AddSingleton<SettingsService>();
    services.AddSingleton<WindowService>();
    services.AddSingleton<NavigationService>();
    services.AddSingleton<DatabaseService>();
    services.AddSingleton<SearchService>();
    services.AddSingleton<NoteService>();
    services.AddSingleton<TagService>();
    services.AddSingleton<DialogService>();

    // DAOs
    services.AddKeyedSingleton<DbDaoBase, BoardDbDao>("BoardDao");
    services.AddKeyedSingleton<DbDaoBase, NoteDbDao>("NoteDao");
    services.AddKeyedSingleton<DbDaoBase, TagDbDao>("TagDao");

    // ViewModels
    services.AddSingleton<MainViewModel>();
    services.AddSingleton<BoardViewModelFactory>();
    services.AddSingleton<NoteViewModelFactory>();
    services.AddSingleton<TagsViewModel>();
    services.AddSingleton<SettingsViewModel>();

    return services.BuildServiceProvider();
  }

  public T GetService<T>() => (T)Services.GetRequiredService(typeof(T));
  #endregion
}