using App;
using DigitalManualSet.App.Common;
using DigitalManualSet.App.Navigation;
using DigitalManualSet.App.Services;
using DigitalManualSet.App.Settings;
using DigitalManualSet.App.ViewModels;
using DigitalManualSet.App.ViewModels.CreatePackage;
using DigitalManualSet.App.ViewModels.CreatePackage.Dialogs;
using DigitalManualSet.App.Views.Dialogs;
using DigitalManualSet.Core.Common;
using DigitalManualSet.Core.Documents;
using DigitalManualSet.Core.Orders;
using DigitalManualSet.Infrastructure.Documents;
using DigitalManualSet.Infrastructure.Orders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;
using System.Windows;
using DigitalManualSet.App.Services.Interfaces;

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
        /// The application path provider
        /// </summary>
        private AppPathProvider _appPathProvider;

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Startup" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs" /> that contains the event data.</param>
        protected override async void OnStartup(StartupEventArgs e)
        {
            _appPathProvider = new AppPathProvider();

            InitLogging();

            Log.Information("Application started");

            // Log PC Info
            var pcInfo =
                $"Machine: {Environment.MachineName}, " +
                $"User: {Environment.UserName}";
            Log.Information(pcInfo);

            ConfigureServices();
            await DepersistSettings();

            var mainWindow = new MainWindow
            {
                DataContext = _serviceProvider.GetRequiredService<ShellViewModel>()
            };
            mainWindow.Show();

            base.OnStartup(e);
        }

        /// <summary>
        /// Depersists the settings file
        /// </summary>
        private async Task DepersistSettings()
        {
            var settingsService = _serviceProvider.GetRequiredService<AppSettingsService>();
            await settingsService.LoadAsync();
            Log.Information("Settings depersisted");

            var themeService = _serviceProvider.GetRequiredService<AppThemeService>();
            themeService.ApplyTheme(settingsService.Settings.Theme);
        }

        /// <summary>
        /// Configures all application services and registers them in the dependency injection container.
        /// This includes logging, navigation, view models, and screen registry.
        /// </summary>
        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // Add logging to our services.
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog();
            });

            // App level
            services.AddSingleton<IAppPathProvider>(_appPathProvider);
            services.AddSingleton<AppSettingsService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<AppThemeService>();
            services.AddSingleton<IPackageWorkflowStepViewModelResolver, PackageWorkflowStepViewModelResolver>();


            services.AddSingleton<IOpenOrderProvider>(
                serviceProvider =>
                {
                    var filePath = Path.Combine(
                        AppContext.BaseDirectory,
                        "TestData",
                        "ValidOpenOrders.csv");

                    return new CsvOpenOrderProvider(filePath);
                });

            services.AddSingleton<IDocumentProvider>(
                serviceProvider =>
                {
                    var filePath = Path.Combine(
                        AppContext.BaseDirectory,
                        "TestData",
                        "TestDocuments.csv");

                    return new CsvDocumentProvider(filePath);
                });

            services.AddSingleton<IDocumentDownloadService>(_ => new LocalDocumentDownloadService(_appPathProvider.TestDocuments));

            services.AddSingleton<IDocumentValidationService, DocumentValidationService>();


            services.AddSingleton<IAddDocumentDialogService, AddDocumentDialogService>();

            services.AddSingleton<IMessageService, MessageService>();

            services.AddSingleton<DarkOverlayService>();

            services.AddTransient<MessageDialog>();

            services.AddTransient<AddDocumentDialog>();
            services.AddTransient<AddDocumentDialogViewModel>();

            services.AddTransient<CreatePackageViewModel>();
            services.AddTransient<CreateOrderViewModel>();
            services.AddTransient<ProcessDocumentsViewModel>();
            services.AddTransient<SelectOutputViewModel>();
            services.AddTransient<CompletePackageViewModel>();

            services.AddTransient<OpenPackageViewModel>();
            services.AddTransient<SettingsViewModel>();

            // Navigation / screen metadata creation
            services.AddSingleton<ShellViewModel>();
            services.AddSingleton<ScreenRegistry>();
            services.AddSingleton<HomeViewModel>();

            _serviceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// Initializes logging configuration, creating the log directory and configuring Serilog
        /// to output daily rolling log files with a 14-day retention policy.
        /// </summary>
        private void InitLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    Path.Combine(_appPathProvider.LogsFolder, $"{AppInfo.ProductName}-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 14)
                .CreateLogger();
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
