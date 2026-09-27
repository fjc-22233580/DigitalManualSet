using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DigitalManualSet.App.Views.Common.Converters;

public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var result = value is true;

        if (parameter is string s && bool.TryParse(s, out var invert) && invert)
        {
            result = !result;
        }

        return result
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}