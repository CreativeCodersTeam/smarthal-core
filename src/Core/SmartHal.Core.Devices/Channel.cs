namespace SmartHal.Core.Devices;

/// <summary>
/// Represents a channel of a device, each with its own set of parameters.
/// </summary>
public class Channel
{
    /// <summary>Gets or sets the channel number.</summary>
    public int Number { get; set; }

    /// <summary>Gets or sets the channel name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the channel parameters.</summary>
    public Dictionary<string, ParameterValue> Parameters { get; set; } = [];
}
