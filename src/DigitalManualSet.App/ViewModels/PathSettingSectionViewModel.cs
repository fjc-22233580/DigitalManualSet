using DigitalManualSet.App.Common;
using System.Diagnostics;
using System.IO;

namespace DigitalManualSet.App.ViewModels;

/// <summary>
/// Represents a single configurable path section shown in the settings UI.
/// It stores the display metadata and the target folder or file path used by the action command.
/// </summary>
public class PathSettingSectionViewModel
{
    public string IconGlyph { get; }
    public string SectionTitle { get; }
    public string SectionDescription { get; }
    public string Path { get; }

    public RelayCommand OpenPathCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PathSettingSectionViewModel"/> class.
    /// </summary>
    /// <param name="iconGlyph">The icon glyph.</param>
    /// <param name="sectionTitle">The section title.</param>
    /// <param name="sectionDescription">The section description.</param>
    /// <param name="path">The path.</param>
    public PathSettingSectionViewModel(string iconGlyph, string sectionTitle, string sectionDescription, string path)
    {
        IconGlyph = iconGlyph;
        SectionTitle = sectionTitle;
        SectionDescription = sectionDescription;
        Path = path;

        OpenPathCommand = new RelayCommand(OpenPath);
    }

    /// <summary>
    /// Opens the folder or parent folder of the given path.
    /// </summary>
    private void OpenPath()
    {
        if (string.IsNullOrWhiteSpace(Path))
            return;

        var folderPath = Directory.Exists(Path) ? Path : System.IO.Path.GetDirectoryName(Path);

        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            return;

        Process.Start(new ProcessStartInfo
        {
            FileName = folderPath,
            UseShellExecute = true
        });
    }
}