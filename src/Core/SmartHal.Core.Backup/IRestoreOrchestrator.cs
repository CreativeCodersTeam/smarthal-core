namespace SmartHal.Core.Backup;

/// <summary>
/// Orchestrates the restore of a snapshot, including a pre-restore safety snapshot
/// and per-device error tolerance.
/// </summary>
public interface IRestoreOrchestrator
{
    /// <summary>Computes a dry-run preview of the changes a restore would apply.</summary>
    /// <param name="snapshotId">The snapshot identifier.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The preview result.</returns>
    Task<RestorePreviewResult> PreviewRestoreAsync(string snapshotId, CancellationToken ct = default);

    /// <summary>Restores a snapshot. Always creates a pre-restore safety snapshot first.</summary>
    /// <param name="snapshotId">The snapshot identifier.</param>
    /// <param name="force">When true, restore even if there would be no changes.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The restore result, including any per-device errors.</returns>
    Task<RestoreResult> RestoreAsync(string snapshotId, bool force = false, CancellationToken ct = default);
}
