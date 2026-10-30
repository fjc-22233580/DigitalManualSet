namespace DigitalManualSet.Core.Services;

public interface IRemovableDriveCleaner
{
    /// <summary>
    /// Clears the given USB drive asynchronous.
    /// </summary>
    /// <param name="rootPath">The root path.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task ClearAsync(string rootPath, CancellationToken cancellationToken = default);
}