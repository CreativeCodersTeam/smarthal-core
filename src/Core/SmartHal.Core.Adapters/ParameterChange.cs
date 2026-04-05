namespace SmartHal.Core.Adapters;

/// <summary>
/// Represents a single parameter change in a restore preview.
/// </summary>
public class ParameterChange
{
    /// <summary>Gets or sets the name of the parameter being changed.</summary>
    public string ParameterName { get; set; } = string.Empty;

    /// <summary>Gets or sets the current value of the parameter.</summary>
    public string? CurrentValue { get; set; }

    /// <summary>Gets or sets the new value that would be applied.</summary>
    public string? NewValue { get; set; }
}
