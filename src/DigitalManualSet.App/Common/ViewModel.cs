using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DigitalManualSet.App.Common;


/// <summary>
/// Provides a base implementation of <see cref="INotifyPropertyChanged"/> for view models.
/// </summary>
/// <remarks>
/// View models can inherit from this class to notify the WPF binding system when
/// property values change. The <see cref="SetProperty{T}"/> helper updates a backing
/// field only when the value has changed and then raises <see cref="PropertyChanged"/>.
/// </remarks>
public class ViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// Occurs when a property value has changed.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Updates a backing field and raises <see cref="PropertyChanged"/> if the value has changed.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="backingField">A reference to the field that stores the property value.</param>
    /// <param name="value">The new value to assign to the backing field.</param>
    /// <param name="propertyName">
    /// The name of the property that changed. This is supplied automatically by the compiler
    /// when omitted by the caller.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the backing field was updated; otherwise, <see langword="false"/>
    /// if the existing value and new value were equal.
    /// </returns>Okay n
    protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingField, value))
        {
            return false;
        }

        backingField = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for the specified property.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the property that changed. This is supplied automatically by the compiler
    /// when omitted by the caller.
    /// </param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}