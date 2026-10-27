using System.Globalization;
using System.Windows.Data;

namespace DigitalManualSet.App.Views.Common.Converters;

/// <summary>
/// Converts byte counts to human-readable binary size strings.
/// </summary>
public sealed class BytesToReadableSizeConverter : IValueConverter
{
    /// <summary>
    /// Converts a byte count to a formatted size using B, KB, MB, or GB units.
    /// </summary>
    /// <param name="value">The byte count to convert. Values must be of type <see cref="long"/>.</param>
    /// <param name="targetType">The requested target type.</param>
    /// <param name="parameter">An optional converter parameter.</param>
    /// <param name="culture">The culture used for formatting the result.</param>
    /// <returns>A formatted size string, or an empty string when <paramref name="value"/> is not a <see cref="long"/>.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not long bytes)
            return string.Empty;

        const double kb = 1024;
        const double mb = kb * 1024;
        const double gb = mb * 1024;

        return bytes switch
        {
            >= (long)gb => $"{bytes / gb:N1} GB",
            >= (long)mb => $"{bytes / mb:N1} MB",
            >= (long)kb => $"{bytes / kb:N1} KB",
            _ => $"{bytes} B"
        };
    }

    /// <summary>
    /// Rejects reverse conversion because readable size strings cannot be converted back by this converter.
    /// </summary>
    /// <param name="value">The value to convert back.</param>
    /// <param name="targetType">The requested target type.</param>
    /// <param name="parameter">An optional converter parameter.</param>
    /// <param name="culture">The culture used for conversion.</param>
    /// <returns>This method does not return because reverse conversion is unsupported.</returns>
    /// <exception cref="NotSupportedException">Always thrown because reverse conversion is unsupported.</exception>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}