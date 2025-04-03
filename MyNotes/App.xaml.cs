using Microsoft.Extensions.DependencyInjection;

using MyNotes.Core.Services;
using MyNotes.Core.ViewModels;
using MyNotes.Core.Views;

namespace MyNotes;

public partial class App : Application
{
  public App()
  {
    this.InitializeComponent();
  }

  protected override void OnLaunched(LaunchActivatedEventArgs args)
  {
    new MainWindow().Activate();
  }

  public static new readonly App Current = (App)Application.Current;

  #region Services
  public ServiceProvider Services { get; } = ConfigureServices();
  public static ServiceProvider ConfigureServices()
  {
    ServiceCollection services = new();

    // Services
    services.AddSingleton<NavigationService>();

    // ViewModels
    services.AddSingleton<MainViewModel>();

    return services.BuildServiceProvider();
  }
  public T GetService<T>() => (T)Services.GetRequiredService(typeof(T));
  #endregion
}