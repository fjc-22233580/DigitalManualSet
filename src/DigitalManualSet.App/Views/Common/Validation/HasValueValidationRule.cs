using System.Globalization;
using System.Windows.Controls;

namespace DigitalManualSet.App.Views.Common.Validation;

public class HasValueValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        return string.IsNullOrWhiteSpace(value?.ToString())
            ? new ValidationResult(false, "This field is required.")
            : ValidationResult.ValidResult;
    }
}