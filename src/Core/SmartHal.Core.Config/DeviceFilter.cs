using SmartHal.Core.Devices;

namespace SmartHal.Core.Config;

/// <summary>
/// Filter criteria for listing devices.
/// </summary>
public class DeviceFilter
{
    /// <summary>Gets or sets the adapter identifier to filter by.</summary>
    public string? AdapterId { get; set; }

    /// <summary>Gets or sets the room identifier to filter by.</summary>
    public string? RoomId { get; set; }

    /// <summary>Gets or sets the device type to filter by.</summary>
    public DeviceType? Type { get; set; }

    /// <summary>Gets or sets the group identifier to filter by.</summary>
    public string? GroupId { get; set; }

    /// <summary>Gets or sets a glob pattern for filtering by device name (e.g. "*licht*").</summary>
    public string? NamePattern { get; set; }
}
