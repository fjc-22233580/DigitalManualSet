using App;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.IO;
using System.Windows;
using Microsoft.Extensions.Logging;

namespace DigitalManualSet.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        public App()
        {
        }


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
                // DataContext = _serviceProvider.GetRequiredService<ShellViewModel>()
            };

            mainWindow.Show();


            base.OnStartup(e);
        }

        private void ConfigureServices(IServiceCollection services)
        {
           
            // Add logging to our services.
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog();
            });
        }

        /// <summary>
        /// Initialise logging.
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

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("Application closing");
            Log.CloseAndFlush();

            base.OnExit(e);
        }
    }
}
