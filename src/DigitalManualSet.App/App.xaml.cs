using App;
using DigitalManualSet.App.Common;
using DigitalManualSet.App.Navigation;
using DigitalManualSet.App.Services;
using DigitalManualSet.App.Services.Interfaces;
using DigitalManualSet.App.Settings;
using DigitalManualSet.App.ViewModels;
using DigitalManualSet.App.ViewModels.CreatePackage;
using DigitalManualSet.App.ViewModels.CreatePackage.Dialogs;
using DigitalManualSet.App.Views.Dialogs;
using DigitalManualSet.Core.Common;
using DigitalManualSet.Core.Documents;
using DigitalManualSet.Core.Orders;
using DigitalManualSet.Core.Services;
using DigitalManualSet.Infrastructure.Documents;
using DigitalManualSet.Infrastructure.Orders;
using DigitalManualSet.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;
using System.Windows;
using DigitalManualSet.Infrastructure;

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

            var documentRegisterProvider = _serviceProvider.GetRequiredService<IDocumentRegisterProvider>();
            await documentRegisterProvider.LoadAsync();

            await DepersistSettings();

            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            await ParseOpenOrders();

            var catalogue = _serviceProvider.GetRequiredService<IDocumentCatalogueService>();
            await catalogue.LoadAsync();

            var mainWindow = new MainWindow
            {
                DataContext = _serviceProvider.GetRequiredService<ShellViewModel>()
            };
            mainWindow.Show();

            base.OnStartup(e);
        }

        /// <summary>
        /// Loads the open orders spreadsheet into the <see cref="SpreadsheetOpenOrderProvider"/> so orders
        /// are available before the main window is shown.
        /// </summary>
        /// <remarks>
        /// In DEBUG builds the spreadsheet is read from the local Documents folder instead of the configured path.
        /// </remarks>
        private async Task ParseOpenOrders()
        {
            var openOrdersService = _serviceProvider.GetRequiredService<SpreadsheetOpenOrderProvider>();
            var appPathsProvide = _serviceProvider.GetRequiredService<IAppPathProvider>();
            var openOrderSpreadsheet = _appPathProvider.OpenOrdersFilePath;

#if DEBUG
            // "...\Documents\Digital Manual Set\MACHINE WORK TO LIST MK2.xlsm"
            openOrderSpreadsheet = Path.Combine(appPathsProvide.DocumentsFolder, "MACHINE WORK TO LIST MK2.xlsm");
#endif
            try
            {
                await openOrdersService.InitializeAsync(openOrderSpreadsheet);

            }
            catch (Exception e)
            {
               // Ignore any exceptions - we show errors to the user later.
            }
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

            // App level: paths, settings, theming and navigation.
            services.AddSingleton<IAppPathProvider>(_appPathProvider);
            services.AddSingleton<AppSettingsService>();
            services.AddSingleton<AppThemeService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Orders: the spreadsheet provider is initialised at startup. IOpenOrderProvider checks
            // DemoMode on each request, so the toggle takes effect at runtime.
            services.AddSingleton<SpreadsheetOpenOrderProvider>();
            services.AddSingleton<IOpenOrderProvider, DemoModeOpenOrderProvider>();
            services.AddTransient<IOrderImportOrchestrator, OrderImportOrchestrator>();

            // Documents: built-in demo documents, register lookup/validation, download and catalogue.
            services.AddSingleton<IDocumentProvider>(_ => new CsvDocumentProvider(
                Path.Combine(AppContext.BaseDirectory, "TestData", "TestDocuments.csv")));
            services.AddSingleton<IDocumentRegisterProvider>(sp => new DemoModeDocumentRegisterProvider(
                sp.GetRequiredService<AppSettingsService>(),
                _appPathProvider.MasterDocumentListFilePath));
            services.AddSingleton<IDocumentRegisterValidator, DocumentRegisterValidator>();
            services.AddSingleton<IDocumentDownloadService>(_ => new LocalDocumentDownloadService(_appPathProvider.TestDocuments));
            services.AddSingleton<IDocumentValidationService, DocumentValidationService>();
            services.AddSingleton<IDocumentCatalogueService>(_ => new DocumentCatalogueService(
                Path.Combine(_appPathProvider.DocumentsFolder, "documents.json")));

            // Package output: generation, validation and removable drive handling.
            services.AddSingleton<IPackageWriter, PackageWriter>();
            services.AddSingleton<IManifestGenerator, ManifestGenerator>();
            services.AddSingleton<ICompletionCertificateGenerator, CompletionCertificateGenerator>();
            services.AddSingleton<ISplashScreenGenerator, SplashScreenGenerator>();
            services.AddSingleton<IPackageValidator, PackageValidator>();
            services.AddSingleton<IRemovableDriveService, RemovableDriveService>();
            services.AddSingleton<IVolumeRenamer, VolumeRenamer>();
            services.AddSingleton<IRemovableDriveCleaner, RemovableDriveCleaner>();

            // UI services and dialogs.
            services.AddSingleton<IMessageService, MessageService>();
            services.AddSingleton<DarkOverlayService>();
            services.AddSingleton<IAddDocumentDialogService, AddDocumentDialogService>();
            services.AddTransient<MessageDialog>();
            services.AddTransient<AddDocumentDialog>();
            services.AddTransient<AddDocumentDialogViewModel>();

            // Create package workflow view models.
            services.AddSingleton<IPackageWorkflowStepViewModelResolver, PackageWorkflowStepViewModelResolver>();
            services.AddTransient<CreatePackageViewModel>();
            services.AddTransient<CreateOrderViewModel>();
            services.AddTransient<ProcessDocumentsViewModel>();
            services.AddTransient<CompletePackageViewModel>();

            // Screen view models.
            services.AddTransient<OpenPackageViewModel>();
            services.AddTransient<SettingsViewModel>();

            // Shell, navigation and screen registry.
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
