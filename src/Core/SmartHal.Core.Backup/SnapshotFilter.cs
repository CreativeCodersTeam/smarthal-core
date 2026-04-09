using SmartHal.Core.Config;

namespace SmartHal.Core.Backup;

/// <summary>
/// Filter criteria for listing snapshots.
/// </summary>
public class SnapshotFilter
{
    /// <summary>Gets or sets the scope filter. Only snapshots with this scope are returned when set.</summary>
    public ConfigScope? Scope { get; set; }

    /// <summary>Gets or sets the scope identifier filter. Only snapshots with this scope id are returned when set.</summary>
    public string? ScopeId { get; set; }

    /// <summary>Gets or sets the trigger filter. Only snapshots with this trigger are returned when set.</summary>
    public string? Trigger { get; set; }

    /// <summary>Gets or sets the lower bound for the creation timestamp (inclusive).</summary>
    public DateTimeOffset? Since { get; set; }

    /// <summary>Gets or sets the upper bound for the creation timestamp (inclusive).</summary>
    public DateTimeOffset? Until { get; set; }
}
