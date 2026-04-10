using CreativeCoders.Cli.Core;
using JetBrains.Annotations;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Backup;
using Spectre.Console;

namespace SmartHal.CLI.Commands.Backup;

/// <summary>
/// Shows details of a specific snapshot.
/// </summary>
[UsedImplicitly]
[CliCommand(["backup", "show"], Name = "show", Description = "Show snapshot details")]
public class BackupShowCommand(
    ISnapshotManager snapshotManager,
    IAnsiConsole console,
    OutputFormatter formatter) : ICliCommand<BackupShowOptions>
{
    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(BackupShowOptions options)
    {
        var manifest = await snapshotManager.GetSnapshotAsync(options.SnapshotId).ConfigureAwait(false);

        formatter.WriteObject(
            manifest,
            ("Snapshot ID", manifest.SnapshotId),
            ("Created", manifest.CreatedAt.ToString("o")),
            ("Scope", manifest.Scope.ToString()),
            ("Scope ID", manifest.ScopeId ?? "-"),
            ("Mode", manifest.Mode.ToString()),
            ("Trigger", manifest.Trigger ?? "-"),
            ("Description", manifest.Description ?? "-"),
            ("Devices", manifest.Entries.Count.ToString()));

        if (manifest.Entries.Count > 0)
        {
            console.WriteLine();
            formatter.WriteTable(
                manifest.Entries.ToList(),
                ("Device ID", e => e.DeviceId),
                ("Adapter", e => e.AdapterId),
                ("Native ID", e => e.NativeId),
                ("Config", e => e.ConfigFilePath));
        }

        return CommandResult.Success;
    }
}
