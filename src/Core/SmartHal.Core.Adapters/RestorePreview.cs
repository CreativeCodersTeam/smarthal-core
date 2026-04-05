namespace SmartHal.Core.Adapters;

/// <summary>
/// Preview of changes that a restore operation would apply to a device.
/// </summary>
public class RestorePreview
{
    /// <summary>Gets or sets the native device identifier.</summary>
    public string NativeId { get; set; } = string.Empty;

    /// <summary>Gets or sets the list of parameter changes.</summary>
    public IReadOnlyList<ParameterChange> Changes { get; set; } = [];

    /// <summary>Gets or sets whether the device needs a restart after restore.</summary>
    public bool RequiresDeviceRestart { get; set; }
}
