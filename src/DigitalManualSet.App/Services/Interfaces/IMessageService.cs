namespace DigitalManualSet.App.Services.Interfaces;

/// <summary>
/// Defines a service for displaying message and question dialogs.
/// </summary>
public interface IMessageService
{
    /// <summary>
    /// Displays a message dialog with the specified title and message.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="message">The message to display.</param>
    void ShowMessage(string title, string message);

    /// <summary>
    /// Displays a question dialog with the specified title and question.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="question">The question to display.</param>
    /// <returns><see langword="true"/> if the user confirms; otherwise, <see langword="false"/>.</returns>
    bool ShowQuestion(string title, string question);
}
