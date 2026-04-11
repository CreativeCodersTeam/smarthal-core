using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
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
    OutputFormatter formatter,
    ILogger<BackupCleanupCommand> logger) : ICliCommand<BackupCleanupOptions>
{
    private readonly ISnapshotManager _snapshotManager = Ensure.NotNull(snapshotManager);
    private readonly OutputFormatter _formatter = Ensure.NotNull(formatter);
    private readonly ILogger<BackupCleanupCommand> _logger = Ensure.NotNull(logger);

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(BackupCleanupOptions options)
    {
        _logger.LogInformation("Running backup cleanup with retention policy");

        var policy = new RetentionPolicy
        {
            MaxSnapshots = options.MaxCount,
            MaxAge = options.MaxAge.HasValue ? TimeSpan.FromDays(options.MaxAge.Value) : null,
            KeepManualSnapshots = options.KeepManual
        };

        var deleted = await _snapshotManager.ApplyRetentionPolicyAsync(policy).ConfigureAwait(false);

        _formatter.WriteSuccess($"Deleted {deleted} snapshot(s).");
        return CommandResult.Success;
    }
}
