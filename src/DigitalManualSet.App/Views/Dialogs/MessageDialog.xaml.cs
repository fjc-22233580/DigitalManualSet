using System.Windows;

namespace DigitalManualSet.App.Views.Dialogs
{
    /// <summary>
    /// Displays a message or question in a modal dialog.
    /// </summary>
    public partial class MessageDialog : Window
    {
        /// <summary>
        /// Identifies the <see cref="Message"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            nameof(Message), typeof(string), typeof(MessageDialog), new PropertyMetadata(default(string)));

        /// <summary>
        /// Gets or sets the message displayed by the dialog.
        /// </summary>
        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the dialog should display Yes and No options.
        /// </summary>
        public bool IsQuestion { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageDialog"/> class.
        /// </summary>
        public MessageDialog()
        {
            InitializeComponent();
            this.Loaded += MessageDialog_Loaded;
        }

        private void MessageDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsQuestion)
            {
                NoButton.Content = "No";
                NoButton.Visibility = Visibility.Visible;
                YesButton.Content = "Yes";
            }
            else
            {
                NoButton.Content = "";
                NoButton.Visibility = Visibility.Collapsed;
                YesButton.Content = "Okay";
            }
        }

        private void YesButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
