namespace SmartHal.Core.Adapters;

/// <summary>
/// Configuration for an adapter instance. Settings may contain secret key references
/// (identified by the "_key" suffix) that are resolved at runtime by the secrets provider.
/// </summary>
public class AdapterConfig
{
    /// <summary>Gets or sets the unique adapter instance identifier.</summary>
    public string AdapterId { get; set; } = string.Empty;

    /// <summary>Gets or sets the adapter type (e.g. "homematic").</summary>
    public string AdapterType { get; set; } = string.Empty;

    /// <summary>Gets or sets the adapter-specific settings.</summary>
    public Dictionary<string, string> Settings { get; set; } = [];
}
