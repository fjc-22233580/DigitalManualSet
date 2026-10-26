using DigitalManualSet.App.Views.Dialogs;
using DigitalManualSet.Core.Documents;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using DigitalManualSet.App.Services.Interfaces;

namespace DigitalManualSet.App.Services;

/// <summary>
/// Provides access to the add-document dialog.
/// </summary>
public class AddDocumentDialogService : IAddDocumentDialogService
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddDocumentDialogService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve the dialog and overlay service.</param>
    public AddDocumentDialogService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Displays the add-document dialog and returns the selected document.
    /// </summary>
    /// <returns>The selected document, or <see langword="null"/> if the dialog is cancelled.</returns>
    public Document? ShowDialog()
    {
        var dialog = _serviceProvider.GetRequiredService<AddDocumentDialog>();
        var overlayService = _serviceProvider.GetRequiredService<DarkOverlayService>();

        dialog.Owner = Application.Current.MainWindow;
        dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

        overlayService.ShowOverlay();
        if (dialog.ShowDialog() != true)
        {
            overlayService.HideOverlay();
            return null;
        }

        overlayService.HideOverlay();
        return dialog.ViewModel.Result;
    }
}