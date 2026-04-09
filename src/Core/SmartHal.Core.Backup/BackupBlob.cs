namespace SmartHal.Core.Backup;

/// <summary>
/// Adapter-agnostic container for an embedded device backup payload.
/// </summary>
/// <remarks>
/// Core.Backup intentionally does not depend on Core.Adapters. Adapters that support
/// backup/restore are surfaced via <see cref="IAdapterBackupCapability"/>, which produces
/// and consumes <see cref="BackupBlob"/> instances. The actual translation between this
/// blob and adapter-specific types is done by the lookup implementation.
/// </remarks>
public class BackupBlob
{
    /// <summary>Gets or sets the native (adapter-specific) device identifier.</summary>
    public string NativeId { get; set; } = string.Empty;

    /// <summary>Gets or sets the adapter type that created the backup.</summary>
    public string AdapterType { get; set; } = string.Empty;

    /// <summary>Gets or sets the timestamp when the backup was created.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets the adapter-specific backup payload.</summary>
    public Dictionary<string, object> Data { get; set; } = [];
}
