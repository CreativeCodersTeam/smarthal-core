namespace SmartHal.Core.Config;

/// <summary>
/// Represents the difference between two states of a device.
/// </summary>
public class DeviceDiff
{
    /// <summary>Gets or sets the device identifier.</summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>Gets a value that indicates whether there are any changes.</summary>
    public bool HasChanges => Changes.Count > 0;

    /// <summary>Gets or sets the list of individual changes.</summary>
    public List<DiffEntry> Changes { get; set; } = [];
}

/// <summary>
/// Represents a single change within a device diff.
/// </summary>
public class DiffEntry
{
    /// <summary>Gets or sets the kind of change.</summary>
    public DiffKind Kind { get; set; }

    /// <summary>Gets or sets the path of the changed value (e.g. "parameters.brightness").</summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>Gets or sets the old value.</summary>
    public string? OldValue { get; set; }

    /// <summary>Gets or sets the new value.</summary>
    public string? NewValue { get; set; }
}

/// <summary>
/// Defines the kind of a diff entry.
/// </summary>
public enum DiffKind
{
    /// <summary>A value was added.</summary>
    Added,

    /// <summary>A value was removed.</summary>
    Removed,

    /// <summary>A value was changed.</summary>
    Changed
}
