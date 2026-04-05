namespace SmartHal.Core.Adapters;

/// <summary>
/// Optional capability for adapters that support backup and restore of device configurations.
/// </summary>
public interface IBackupRestore
{
    /// <summary>Creates a backup of a single device's configuration.</summary>
    Task<AdapterBackup> BackupDeviceAsync(string nativeId, CancellationToken ct = default);

    /// <summary>Previews the changes that a restore operation would apply.</summary>
    Task<RestorePreview> PreviewRestoreAsync(string nativeId, AdapterBackup backup, CancellationToken ct = default);

    /// <summary>Restores a device's configuration from a backup.</summary>
    Task RestoreDeviceAsync(string nativeId, AdapterBackup backup, CancellationToken ct = default);
}
