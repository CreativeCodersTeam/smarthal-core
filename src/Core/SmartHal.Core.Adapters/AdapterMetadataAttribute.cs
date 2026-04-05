namespace SmartHal.Core.Adapters;

/// <summary>
/// Decorates an <see cref="ISmartHalAdapter"/> implementation class with its adapter type
/// metadata. Used for automatic adapter discovery via assembly scanning.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class AdapterMetadataAttribute : Attribute
{
    /// <summary>Gets the adapter type identifier (e.g. "homematic").</summary>
    public string AdapterType { get; }

    /// <summary>Gets the human-readable display name.</summary>
    public string DisplayName { get; }

    /// <summary>Gets or sets an optional description of the adapter.</summary>
    public string? Description { get; set; }

    public AdapterMetadataAttribute(string adapterType, string displayName)
    {
        AdapterType = adapterType;
        DisplayName = displayName;
    }
}
