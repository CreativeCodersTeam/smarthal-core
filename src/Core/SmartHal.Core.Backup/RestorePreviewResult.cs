using SmartHal.Core.Config;

namespace SmartHal.Core.Backup;

/// <summary>
/// Result of a dry-run restore preview. Lists the changes that would be applied per device.
/// </summary>
public class RestorePreviewResult
{
    /// <summary>Gets or sets the snapshot identifier.</summary>
    public string SnapshotId { get; set; } = string.Empty;

    /// <summary>Gets or sets the per-device previews.</summary>
    public List<DeviceRestorePreview> Devices { get; set; } = [];

    /// <summary>Gets a value that indicates whether any device in the snapshot would be changed.</summary>
    public bool HasChanges => Devices.Any(d => d.Diff.HasChanges);
}

/// <summary>
/// Per-device restore preview entry.
/// </summary>
public class DeviceRestorePreview
{
    /// <summary>Gets or sets the device identifier.</summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>Gets or sets the diff between the current device state and the snapshot state.</summary>
    public DeviceDiff Diff { get; set; } = new DeviceDiff();

    /// <summary>
    /// Gets or sets a value indicating whether the device still exists in the live configuration.
    /// When false, the device was deleted between the snapshot and the restore preview.
    /// </summary>
    public bool DeviceExists { get; set; }
}
