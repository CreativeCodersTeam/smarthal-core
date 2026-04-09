namespace SmartHal.Core.Backup;

/// <summary>
/// Resolves adapter backup capabilities by adapter id.
/// </summary>
/// <remarks>
/// This interface keeps Core.Backup independent of Core.Adapters. Implementations bridge
/// the gap between an actual adapter (which may implement IBackupRestore) and the
/// <see cref="IAdapterBackupCapability"/> abstraction used inside this module.
/// </remarks>
public interface IAdapterLookup
{
    /// <summary>
    /// Returns the backup capability for the given adapter, or <c>null</c> when the adapter
    /// is unknown or does not support backups.
    /// </summary>
    /// <param name="adapterId">The adapter identifier.</param>
    /// <returns>The backup capability or <c>null</c>.</returns>
    IAdapterBackupCapability? GetBackupCapability(string adapterId);
}
