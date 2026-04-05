namespace SmartHal.Core.Adapters;

/// <summary>
/// Holds backup data for a single device's configuration.
/// </summary>
public class AdapterBackup
{
    /// <summary>Gets or sets the native device identifier.</summary>
    public string NativeId { get; set; } = string.Empty;

    /// <summary>Gets or sets the adapter type that created this backup.</summary>
    public string AdapterType { get; set; } = string.Empty;

    /// <summary>Gets or sets the timestamp when this backup was created.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets the adapter-specific backup data.</summary>
    public Dictionary<string, object> Data { get; set; } = [];
}
