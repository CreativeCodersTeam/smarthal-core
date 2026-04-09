namespace SmartHal.Core.Backup;

/// <summary>
/// Manages the lifecycle of configuration snapshots: create, list, retrieve, delete and retention.
/// </summary>
public interface ISnapshotManager
{
    /// <summary>Creates a new snapshot from the given request.</summary>
    /// <param name="request">The snapshot request describing scope and mode.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The manifest of the created snapshot.</returns>
    Task<SnapshotManifest> CreateSnapshotAsync(SnapshotRequest request, CancellationToken ct = default);

    /// <summary>Lists snapshots, optionally filtered.</summary>
    /// <param name="filter">An optional filter. When null, all snapshots are returned.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A read-only list of matching snapshot manifests.</returns>
    Task<IReadOnlyList<SnapshotManifest>> ListSnapshotsAsync(SnapshotFilter? filter = null, CancellationToken ct = default);

    /// <summary>Reads a single snapshot manifest.</summary>
    /// <param name="snapshotId">The snapshot identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The snapshot manifest.</returns>
    Task<SnapshotManifest> GetSnapshotAsync(string snapshotId, CancellationToken ct = default);

    /// <summary>Deletes a snapshot and all of its files.</summary>
    /// <param name="snapshotId">The snapshot identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    Task DeleteSnapshotAsync(string snapshotId, CancellationToken ct = default);

    /// <summary>Applies a retention policy and removes snapshots that no longer qualify.</summary>
    /// <param name="policy">The retention policy.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The number of snapshots that were deleted.</returns>
    Task<int> ApplyRetentionPolicyAsync(RetentionPolicy policy, CancellationToken ct = default);
}
