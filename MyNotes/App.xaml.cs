using Microsoft.Extensions.DependencyInjection;

using MyNotes.Views;

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

    // ViewModels

    return services.BuildServiceProvider();
  }
  public T GetService<T>() => (T)Services.GetRequiredService(typeof(T));
  #endregion
}