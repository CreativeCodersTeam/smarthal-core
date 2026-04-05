using SmartHal.Core.Devices;

namespace SmartHal.Core.Config;

/// <summary>
/// Lightweight representation of a device containing only header data.
/// Used for efficient listing without fully parsing each YAML file.
/// </summary>
public class DeviceSummary
{
    /// <summary>Gets or sets the device identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the adapter identifier.</summary>
    public string AdapterId { get; set; } = string.Empty;

    /// <summary>Gets or sets the native device identifier.</summary>
    public string NativeId { get; set; } = string.Empty;

    /// <summary>Gets or sets the device type.</summary>
    public DeviceType Type { get; set; }

    /// <summary>Gets or sets the device name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the room identifier.</summary>
    public string? RoomId { get; set; }

    /// <summary>Gets or sets the YAML file path of this device.</summary>
    public string FilePath { get; set; } = string.Empty;
}
