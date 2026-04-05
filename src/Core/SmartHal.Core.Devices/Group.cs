namespace SmartHal.Core.Devices;

/// <summary>
/// Represents a logical group of devices.
/// </summary>
public class Group
{
    /// <summary>Gets or sets the group identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the group name.</summary>
    public string Name { get; set; } = string.Empty;
}
