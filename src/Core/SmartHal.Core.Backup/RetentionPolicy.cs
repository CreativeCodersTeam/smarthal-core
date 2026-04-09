namespace SmartHal.Core.Backup;

/// <summary>
/// Defines the rules used to clean up old snapshots.
/// </summary>
public class RetentionPolicy
{
    /// <summary>Gets or sets the maximum number of snapshots to keep. Older snapshots are removed first when exceeded.</summary>
    public int? MaxSnapshots { get; set; }

    /// <summary>Gets or sets the maximum age of snapshots. Snapshots older than this are removed.</summary>
    public TimeSpan? MaxAge { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether manual snapshots should be exempt from retention.
    /// Defaults to <c>true</c> so users do not lose explicitly created snapshots.
    /// </summary>
    public bool KeepManualSnapshots { get; set; } = true;
}
