using App;
using DigitalManualSet.App.Navigation;
using DigitalManualSet.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;
using System.Windows;

namespace DigitalManualSet.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// The dependency injection service provider for resolving application services.
        /// </summary>
        private ServiceProvider _serviceProvider;


        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Startup" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs" /> that contains the event data.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            // Set app theme to dark
#pragma warning disable WPF0001
            App.Current.ThemeMode = ThemeMode.Dark;
#pragma warning restore WPF0001

            InitLogging();

            var services = new ServiceCollection();

            ConfigureServices(services);

            _serviceProvider = services.BuildServiceProvider();

            var mainWindow = new MainWindow
            {
                DataContext = _serviceProvider.GetRequiredService<ShellViewModel>()
            };

            mainWindow.Show();


            base.OnStartup(e);
        }

        /// <summary>
        /// Configures all application services and registers them in the dependency injection container.
        /// This includes logging, navigation, view models, and screen registry.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        private void ConfigureServices(IServiceCollection services)
        {

            // Add logging to our services.
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog();
            });

            services.AddSingleton<INavigationService, NavigationService>();

            services.AddSingleton<ShellViewModel>();

            // Navigation / screen metadata creation
            services.AddSingleton<ScreenRegistry>();

            services.AddSingleton<HomeViewModel>();
            services.AddSingleton<CreatePackageViewModel>();
            services.AddSingleton<OpenPackageViewModel>();


        }

        /// <summary>
        /// Initializes logging configuration, creating the log directory and configuring Serilog
        /// to output daily rolling log files with a 14-day retention policy.
        /// </summary>
        private void InitLogging()
        {
            var logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "DigitalManualSet",
                "Logs");

            Directory.CreateDirectory(logDirectory);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    Path.Combine(logDirectory, $"{AppInfo.ProductName}-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 14)
                .CreateLogger();

            Log.Information("Application starting");

        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Exit" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.Windows.ExitEventArgs" /> that contains the event data.</param>
        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("Application closing");
            Log.CloseAndFlush();

            base.OnExit(e);
        }
    }
}
