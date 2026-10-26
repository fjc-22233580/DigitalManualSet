namespace DigitalManualSet.App.Common;

/// <summary>
/// Defines a window that can display or hide a dark overlay.
/// </summary>
public interface IOverlayWindow
{
    /// <summary>
    /// Gets or sets a value indicating whether the dark overlay is displayed.
    /// </summary>
    public bool ShowDarkOverlay { get; set; }
}
