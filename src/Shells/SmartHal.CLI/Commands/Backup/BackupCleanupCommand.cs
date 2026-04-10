using CreativeCoders.Cli.Core;
using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Backup;

namespace SmartHal.CLI.Commands.Backup;

/// <summary>Options for the backup cleanup command.</summary>
public class BackupCleanupOptions
{
    /// <summary>Maximum age in days.</summary>
    [OptionParameter('a', "max-age", HelpText = "Maximum age in days")]
    public int? MaxAge { get; set; }

    /// <summary>Maximum number of snapshots to keep.</summary>
    [OptionParameter('c', "max-count", HelpText = "Maximum number of snapshots to keep")]
    public int? MaxCount { get; set; }

    /// <summary>Keep manual snapshots regardless of policy.</summary>
    [OptionParameter('k', "keep-manual", HelpText = "Keep manual snapshots")]
    public bool KeepManual { get; set; }
}

/// <summary>
/// Applies a retention policy to clean up old snapshots.
/// </summary>
[UsedImplicitly]
[CliCommand(["backup", "cleanup"], Name = "cleanup", Description = "Apply retention policy to snapshots")]
public class BackupCleanupCommand(ISnapshotManager snapshotManager) : ICliCommand<BackupCleanupOptions>
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

        OutputFormatter.WriteSuccess($"Deleted {deleted} snapshot(s).");
        return CommandResult.Success;
    }
}
