namespace DigitalManualSet.Core.Documents;

public enum DocumentCategory
{
    /// <summary>
    /// This document is specific to the package order.
    /// </summary>
    OrderSpecific,

    /// <summary>
    /// This document is generic, or rather, specific to the system type but not the order.
    /// </summary>
    Generic
}