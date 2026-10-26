using DigitalManualSet.App.ViewModels.CreatePackage.Dialogs;
using System.Windows;

namespace DigitalManualSet.App.Views.Dialogs
{
    /// <summary>
    /// Dialog for entering a custom document and selecting its file.
    /// </summary>
    public partial class AddDocumentDialog : Window
    {
        /// <summary>
        /// Gets the view model used by the dialog.
        /// </summary>
        public AddDocumentDialogViewModel ViewModel { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddDocumentDialog"/> class.
        /// </summary>
        /// <param name="viewModel">The view model used by the dialog.</param>
        public AddDocumentDialog(AddDocumentDialogViewModel viewModel)
        {
            InitializeComponent();

            ViewModel = viewModel;
            DataContext = viewModel;
        }

        /// <summary>
        /// Attempts to add the document and closes the dialog when successful.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.TryAdd())
            {
                return;
            }

            DialogResult = true;
        }

        /// <summary>
        /// Opens a file picker and updates the selected document file path.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select Document",
                Filter = "PDF Documents (*.pdf)|*.pdf|All Files (*.*)|*.*",
                Multiselect = false
            };

            if (dialog.ShowDialog(this) == true)
                ViewModel.FilePath = dialog.FileName;
        }
    }
}
