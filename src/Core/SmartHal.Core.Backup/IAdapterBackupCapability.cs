namespace SmartHal.Core.Backup;

/// <summary>
/// Decoupled view of an adapter that supports device backups.
/// Implementations typically wrap an adapter-side IBackupRestore.
/// </summary>
public interface IAdapterBackupCapability
{
    /// <summary>Creates a backup of a single device.</summary>
    /// <param name="nativeId">The native (adapter-specific) device identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The backup payload.</returns>
    Task<BackupBlob> BackupDeviceAsync(string nativeId, CancellationToken ct = default);

    /// <summary>Restores a single device from a backup payload.</summary>
    /// <param name="nativeId">The native (adapter-specific) device identifier.</param>
    /// <param name="blob">The backup payload to restore.</param>
    /// <param name="ct">The cancellation token.</param>
    Task RestoreDeviceAsync(string nativeId, BackupBlob blob, CancellationToken ct = default);
}
