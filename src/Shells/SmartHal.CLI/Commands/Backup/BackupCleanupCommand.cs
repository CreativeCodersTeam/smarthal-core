using CreativeCoders.Cli.Core;
using JetBrains.Annotations;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Backup;

namespace SmartHal.CLI.Commands.Backup;

/// <summary>
/// Applies a retention policy to clean up old snapshots.
/// </summary>
[UsedImplicitly]
[CliCommand(["backup", "cleanup"], Name = "cleanup", Description = "Apply retention policy to snapshots")]
public class BackupCleanupCommand(
    ISnapshotManager snapshotManager,
    OutputFormatter formatter) : ICliCommand<BackupCleanupOptions>
{
    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(BackupCleanupOptions options)
    {
        var policy = new RetentionPolicy
        {
            MaxSnapshots = options.MaxCount,
            MaxAge = options.MaxAge.HasValue ? TimeSpan.FromDays(options.MaxAge.Value) : null,
            KeepManualSnapshots = options.KeepManual
        };

        var deleted = await snapshotManager.ApplyRetentionPolicyAsync(policy).ConfigureAwait(false);

        formatter.WriteSuccess($"Deleted {deleted} snapshot(s).");
        return CommandResult.Success;
    }
}
