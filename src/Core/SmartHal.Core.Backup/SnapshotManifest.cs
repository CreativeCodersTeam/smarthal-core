using SmartHal.Core.Config;

namespace SmartHal.Core.Backup;

/// <summary>
/// Describes a snapshot of one or more device configurations.
/// </summary>
public class SnapshotManifest
{
    /// <summary>Gets or sets the unique snapshot identifier (also serves as directory name).</summary>
    public string SnapshotId { get; set; } = string.Empty;

    /// <summary>Gets or sets the timestamp when the snapshot was created.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets the configuration scope captured by the snapshot.</summary>
    public ConfigScope Scope { get; set; }

    /// <summary>
    /// Gets or sets the scope identifier (device id, adapter id, room id).
    /// Null when <see cref="Scope"/> is <see cref="ConfigScope.All"/>.
    /// </summary>
    public string? ScopeId { get; set; }

    /// <summary>Gets or sets the snapshot mode (reference vs. embedded).</summary>
    public SnapshotMode Mode { get; set; }

    /// <summary>Gets or sets a free-form description supplied by the caller.</summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the trigger that initiated the snapshot.
    /// Common values: "manual", "pre-restore", "pre-replace".
    /// </summary>
    public string? Trigger { get; set; }

    /// <summary>Gets or sets the list of device entries contained in the snapshot.</summary>
    public List<ManifestEntry> Entries { get; set; } = [];
}
