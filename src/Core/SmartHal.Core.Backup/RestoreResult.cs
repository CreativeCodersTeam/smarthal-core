namespace SmartHal.Core.Backup;

/// <summary>
/// Result of a restore operation. Errors per device are collected and do not stop the overall process.
/// </summary>
public class RestoreResult
{
    /// <summary>Gets or sets the snapshot identifier.</summary>
    public string SnapshotId { get; set; } = string.Empty;

    /// <summary>Gets or sets the number of devices that were successfully restored.</summary>
    public int DevicesRestored { get; set; }

    /// <summary>Gets or sets the number of devices that were skipped (e.g. because they no longer exist).</summary>
    public int DevicesSkipped { get; set; }

    /// <summary>Gets or sets the list of per-device errors that occurred during restore.</summary>
    public List<RestoreError> Errors { get; set; } = [];

    /// <summary>Gets a value indicating whether the restore completed without any errors.</summary>
    public bool Success => Errors.Count == 0;
}

/// <summary>
/// Describes a single error that occurred while restoring a device.
/// </summary>
public class RestoreError
{
    /// <summary>Gets or sets the device identifier.</summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>Gets or sets the human readable error message.</summary>
    public string Message { get; set; } = string.Empty;
}
