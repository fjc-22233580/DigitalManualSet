using System.Diagnostics;
using System.Windows;

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
            base.OnStartup(e);

            // Set app theme to dark
#pragma warning disable WPF0001
            App.Current.ThemeMode = ThemeMode.Dark;
#pragma warning restore WPF0001
        }
    }
}
