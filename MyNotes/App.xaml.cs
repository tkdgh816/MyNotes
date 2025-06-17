using Microsoft.Extensions.DependencyInjection;

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
  }

  public static new readonly App Current = (App)Application.Current;

  #region Services
  public ServiceProvider Services { get; } = ConfigureServices();
  private static ServiceProvider ConfigureServices()
  {
    ServiceCollection services = new();

    // Services
    services.AddSingleton<WindowService>();
    services.AddSingleton<NavigationService>();
    services.AddSingleton<DatabaseService>();
    services.AddSingleton<NoteService>();
    services.AddSingleton<TagService>();
    services.AddSingleton<DialogService>();

    // ViewModels
    services.AddSingleton<MainViewModel>();
    services.AddSingleton<BoardViewModelFactory>();
    services.AddSingleton<NoteViewModelFactory>();
    services.AddSingleton<TagsViewModel>();

    return services.BuildServiceProvider();
  }

  public T GetService<T>() => (T)Services.GetRequiredService(typeof(T));
  #endregion
}