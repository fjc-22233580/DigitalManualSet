namespace DigitalManualSet.App.Common;

/// <summary>
/// Represents a simple view model for an item displayed in a ComboBox.
/// </summary>
/// <typeparam name="T">The type of the underlying value for the item.</typeparam>
public class ComboBoxItemViewModel<T>
{
    /// <summary>
    /// Gets the underlying object associated with this item.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Gets the text displayed to the user for this item.
    /// </summary>
    public string DisplayText { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComboBoxItemViewModel{T}"/> class.
    /// </summary>
    /// <param name="value">The underlying value for the item.</param>
    /// <param name="displayText">The text shown to the user for this item.</param>
    public ComboBoxItemViewModel(T value, string displayText)
    {
        Value = value;
        DisplayText = displayText;
    }
}
