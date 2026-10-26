using System.Windows;
using DigitalManualSet.App.Common;

namespace App
{
    /// <summary>
    /// Represents the application's main window and its dark-overlay state.
    /// </summary>
    /// <remarks>
    /// Implements <see cref="IOverlayWindow"/> so services can control the overlay displayed while modal dialogs are open.
    /// </remarks>
    public partial class MainWindow : Window, IOverlayWindow
    {
        /// <summary>
        /// Identifies the <see cref="ShowDarkOverlay"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowDarkOverlayProperty = DependencyProperty.Register(
            nameof(ShowDarkOverlay), typeof(bool), typeof(MainWindow), new PropertyMetadata(default(bool)));

        /// <summary>
        /// Gets or sets a value indicating whether the dark overlay is displayed.
        /// </summary>
        public bool ShowDarkOverlay
        {
            get { return (bool)GetValue(ShowDarkOverlayProperty); }
            set { SetValue(ShowDarkOverlayProperty, value); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}