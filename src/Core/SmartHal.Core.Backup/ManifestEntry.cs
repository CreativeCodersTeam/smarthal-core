namespace SmartHal.Core.Backup;

/// <summary>
/// Describes a single device that is part of a snapshot.
/// </summary>
public class ManifestEntry
{
    /// <summary>Gets or sets the device identifier.</summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>Gets or sets the adapter identifier the device belongs to.</summary>
    public string AdapterId { get; set; } = string.Empty;

    /// <summary>Gets or sets the native (adapter-specific) device identifier.</summary>
    public string NativeId { get; set; } = string.Empty;

    /// <summary>Gets or sets the path to the device YAML file inside the snapshot directory (relative).</summary>
    public string ConfigFilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path to the adapter backup file inside the snapshot directory (relative).
    /// Only set when the snapshot was created in <see cref="SnapshotMode.Embedded"/> mode and the adapter supports backups.
    /// </summary>
    public string? AdapterBackupPath { get; set; }
}
