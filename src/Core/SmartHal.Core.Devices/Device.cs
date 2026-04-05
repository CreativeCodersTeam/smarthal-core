namespace SmartHal.Core.Devices;

/// <summary>
/// Represents a smart home device with its channels, relations, and parameters.
/// </summary>
public class Device
{
    /// <summary>Gets or sets the unique SmartHal identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the identifier of the adapter managing this device.</summary>
    public string AdapterId { get; set; } = string.Empty;

    /// <summary>Gets or sets the adapter-native device identifier.</summary>
    public string NativeId { get; set; } = string.Empty;

    /// <summary>Gets or sets the device type.</summary>
    public DeviceType Type { get; set; }

    /// <summary>Gets or sets the human-readable device name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the room identifier this device belongs to, if any.</summary>
    public string? RoomId { get; set; }

    /// <summary>Gets or sets the group identifiers this device belongs to.</summary>
    public List<string> GroupIds { get; set; } = [];

    /// <summary>Gets or sets the device-level parameters.</summary>
    public Dictionary<string, ParameterValue> Parameters { get; set; } = [];

    /// <summary>Gets or sets the device channels.</summary>
    public List<Channel> Channels { get; set; } = [];

    /// <summary>Gets or sets the device relations.</summary>
    public List<Relation> Relations { get; set; } = [];
}
