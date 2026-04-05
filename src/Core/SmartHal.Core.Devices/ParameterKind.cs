namespace SmartHal.Core.Devices;

/// <summary>
/// Defines the possible value types for device parameters.
/// </summary>
public enum ParameterKind
{
    /// <summary>A string value.</summary>
    String,

    /// <summary>A numeric value.</summary>
    Number,

    /// <summary>A boolean value.</summary>
    Boolean,

    /// <summary>A value from an adapter-specific enumeration.</summary>
    Enum
}
