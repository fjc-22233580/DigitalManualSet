using DigitalManualSet.App.Services.Interfaces;
using DigitalManualSet.App.Views.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace DigitalManualSet.App.Services;

/// <summary>
/// Provides services for displaying message and question dialogs.
/// </summary>
public class MessageService : IMessageService
{
    #region Fields

    private readonly IServiceProvider _serviceProvider;
    private readonly DarkOverlayService _darkOverlayService;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve the dialog and overlay service.</param>
    public MessageService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _darkOverlayService = _serviceProvider.GetRequiredService<DarkOverlayService>();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Displays a message dialog with the specified title and message.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="message">The message to display.</param>
    public void ShowMessage(string title, string message)
    {
        var dialog = _serviceProvider.GetRequiredService<MessageDialog>();
        dialog.IsQuestion = false;
        dialog.Title = title;
        dialog.Message = message;
        dialog.Owner = Application.Current.MainWindow;
        dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

        _darkOverlayService.ShowOverlay();
        dialog.ShowDialog();
        _darkOverlayService.HideOverlay();

    }

    /// <summary>
    /// Displays a question dialog with the specified title and question.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="question">The question to display.</param>
    /// <returns><see langword="true"/> if the user confirms; otherwise, <see langword="false"/>.</returns>
    public bool ShowQuestion(string title, string question)
    {
        var dialog = _serviceProvider.GetRequiredService<MessageDialog>();
        dialog.IsQuestion = true;
        dialog.Title = title;
        dialog.Message = question;
        dialog.Owner = Application.Current.MainWindow;
        dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

        _darkOverlayService.ShowOverlay();
        var response = dialog.ShowDialog();
        if (response == true)
        {
            _darkOverlayService.HideOverlay();
            return true;
        }

        _darkOverlayService.HideOverlay();
        return false;
    }

    #endregion
}