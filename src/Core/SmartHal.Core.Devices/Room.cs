namespace SmartHal.Core.Devices;

/// <summary>
/// Represents a room that devices can be assigned to.
/// </summary>
public class Room
{
    /// <summary>Gets or sets the room identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the room name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the floor this room is on, if any.</summary>
    public string? Floor { get; set; }
}
