using System.Diagnostics;
using System.IO;
using System.Windows;
using Serilog;

namespace DigitalManualSet.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
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

            base.OnStartup(e);
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
