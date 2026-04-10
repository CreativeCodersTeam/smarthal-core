using CreativeCoders.Cli.Core;
using JetBrains.Annotations;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Backup;
using SmartHal.Core.Config;

namespace SmartHal.CLI.Commands.Backup;

/// <summary>
/// Lists existing snapshots.
/// </summary>
[UsedImplicitly]
[CliCommand(["backup", "list"], Name = "list", Description = "List snapshots")]
public class BackupListCommand(
    ISnapshotManager snapshotManager,
    OutputFormatter formatter) : ICliCommand<BackupListOptions>
{
    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(BackupListOptions options)
    {
        var filter = new SnapshotFilter();

        if (options.Scope is not null && Enum.TryParse<ConfigScope>(options.Scope, true, out var scope))
        {
            filter.Scope = scope;
        }

        if (options.Since is not null && DateTimeOffset.TryParse(options.Since, out var since))
        {
            filter.Since = since;
        }

        var snapshots = await snapshotManager.ListSnapshotsAsync(filter).ConfigureAwait(false);

        formatter.WriteTable(
            snapshots.ToList(),
            ("ID", s => s.SnapshotId),
            ("Created", s => s.CreatedAt.ToString("yyyy-MM-dd HH:mm")),
            ("Scope", s => s.Scope.ToString()),
            ("Trigger", s => s.Trigger ?? "-"),
            ("Devices", s => s.Entries.Count.ToString()),
            ("Description", s => s.Description ?? "-"));

        return CommandResult.Success;
    }
}
