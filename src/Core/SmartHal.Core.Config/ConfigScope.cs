namespace SmartHal.Core.Config;

/// <summary>
/// Defines the scope of a configuration operation.
/// </summary>
public enum ConfigScope
{
    /// <summary>A single device.</summary>
    Device,

    /// <summary>All devices of an adapter.</summary>
    Adapter,

    /// <summary>All devices in a room.</summary>
    Room,

    /// <summary>All devices.</summary>
    All
}
