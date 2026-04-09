using SmartHal.Core.Config;

namespace SmartHal.Core.Backup;

/// <summary>
/// Describes a request to create a new snapshot.
/// </summary>
public class SnapshotRequest
{
    /// <summary>Gets or sets the configuration scope to snapshot.</summary>
    public ConfigScope Scope { get; set; }

    /// <summary>Gets or sets the scope identifier (device id, adapter id, room id). Null when <see cref="Scope"/> is <see cref="ConfigScope.All"/>.</summary>
    public string? ScopeId { get; set; }

    /// <summary>Gets or sets the snapshot mode. Defaults to <see cref="SnapshotMode.Reference"/>.</summary>
    public SnapshotMode Mode { get; set; } = SnapshotMode.Reference;

    /// <summary>Gets or sets a free-form description supplied by the caller.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the trigger that initiated the snapshot. Defaults to "manual".</summary>
    public string Trigger { get; set; } = "manual";
}
