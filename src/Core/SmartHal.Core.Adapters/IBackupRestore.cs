namespace SmartHal.Core.Adapters;

/// <summary>
/// Optional capability for adapters that support backup and restore of device configurations.
/// </summary>
public interface IBackupRestore
{
    /// <summary>Creates a backup of a single device's configuration.</summary>
    /// <param name="nativeId">The native device identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The backup data for the device.</returns>
    Task<AdapterBackup> BackupDeviceAsync(string nativeId, CancellationToken ct = default);

    /// <summary>Previews the changes that a restore operation would apply.</summary>
    /// <param name="nativeId">The native device identifier.</param>
    /// <param name="backup">The backup data to preview.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A preview of the changes that would be applied.</returns>
    Task<RestorePreview> PreviewRestoreAsync(string nativeId, AdapterBackup backup, CancellationToken ct = default);

    /// <summary>Restores a device's configuration from a backup.</summary>
    /// <param name="nativeId">The native device identifier.</param>
    /// <param name="backup">The backup data to restore.</param>
    /// <param name="ct">The cancellation token.</param>
    Task RestoreDeviceAsync(string nativeId, AdapterBackup backup, CancellationToken ct = default);
}
