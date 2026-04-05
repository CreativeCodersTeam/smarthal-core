namespace SmartHal.Core.Devices;

/// <summary>
/// Represents a relation between two devices.
/// </summary>
public class Relation
{
    /// <summary>Gets or sets the relation identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the relation type.</summary>
    public RelationType Type { get; set; }

    /// <summary>Gets or sets the target device identifier.</summary>
    public string TargetId { get; set; } = string.Empty;

    /// <summary>Gets or sets a value that indicates whether the relation is symmetric.</summary>
    public bool Symmetric { get; set; }

    /// <summary>Gets or sets the relation parameters.</summary>
    public Dictionary<string, ParameterValue> Parameters { get; set; } = [];
}
