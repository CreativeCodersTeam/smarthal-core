using SmartHal.Core.Devices;

namespace SmartHal.Core.Config;

/// <summary>
/// Represents the content of <c>rooms.yaml</c> — room and group definitions.
/// </summary>
public class RoomsConfig
{
    /// <summary>Gets or sets the list of rooms.</summary>
    public List<Room> Rooms { get; set; } = [];

    /// <summary>Gets or sets the list of groups.</summary>
    public List<Group> Groups { get; set; } = [];
}
