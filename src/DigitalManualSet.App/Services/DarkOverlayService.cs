using System.Windows;
using DigitalManualSet.App.Common;

namespace DigitalManualSet.App.Services;

/// <summary>
/// Controls the dark overlay displayed by the main window.
/// </summary>
public class DarkOverlayService
{
    /// <summary>
    /// Displays the dark overlay when the main window supports it.
    /// </summary>
    public void ShowOverlay()
    {
        if (Application.Current.MainWindow is IOverlayWindow window)
        {
            window.ShowDarkOverlay = true;
        }
    }

    /// <summary>
    /// Hides the dark overlay when the main window supports it.
    /// </summary>
    public void HideOverlay()
    {
        if (Application.Current.MainWindow is IOverlayWindow window)
        {
            window.ShowDarkOverlay = false;
        }
    }
}