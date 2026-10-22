namespace DigitalManualSet.Core.Orders;

/// <summary>
/// Defines the System ID prefixes that identify supported system types.
/// </summary>
public static class SystemIdPrefixes
{
    /// <summary>
    /// The System ID prefixes for supported system types.
    /// All other System IDs (e.g. "STOCK" or blank values) are not supported.
    /// </summary>
    public static IReadOnlyList<string> ValidPrefixes { get; } =
    [
        "E1", "E3", "E10", "E20",
        "8800",
        "8801", "8802",
        "8803", "8804", "8805", "8806", "8808",
        "8862", "8872", "8874", "8852", "8854",
        "VHS", "ETMT"
    ];

    /// <summary>
    /// Determines whether a System ID belongs to a supported system type.
    /// </summary>
    /// <param name="systemId">The System ID to check.</param>
    /// <returns>
    /// <see langword="true"/> when the System ID starts with one of the
    /// <see cref="ValidPrefixes"/>; otherwise <see langword="false"/>.
    /// </returns>
    public static bool IsValid(string? systemId)
    {
        if (string.IsNullOrWhiteSpace(systemId))
        {
            return false;
        }

        return ValidPrefixes.Any(prefix => systemId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the prefix that a System ID belongs to.
    /// </summary>
    /// <remarks>
    /// When more than one prefix matches (e.g. "E1" and "E10" both match "E10LMTB17882"),
    /// the longest prefix is returned so that each System ID belongs to exactly one prefix.
    /// </remarks>
    /// <param name="systemId">The System ID to check.</param>
    /// <returns>
    /// The longest matching prefix from <see cref="ValidPrefixes"/>, or
    /// <see langword="null"/> when no prefix matches.
    /// </returns>
    public static string? GetPrefix(string? systemId)
    {
        if (string.IsNullOrWhiteSpace(systemId))
        {
            return null;
        }

        string? bestMatch = null;

        foreach (var prefix in ValidPrefixes)
        {
            if (systemId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
                (bestMatch is null || prefix.Length > bestMatch.Length))
            {
                bestMatch = prefix;
            }
        }

        return bestMatch;
    }
}
