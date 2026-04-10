using CreativeCoders.Cli.Core;
using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Backup;

namespace SmartHal.CLI.Commands.Backup;

/// <summary>Options for the backup show command.</summary>
public class BackupShowOptions
{
    /// <summary>The snapshot ID to display.</summary>
    [OptionValue(0, HelpText = "The snapshot ID to display")]
    public string SnapshotId { get; set; } = string.Empty;
}

/// <summary>
/// Shows details of a specific snapshot.
/// </summary>
[UsedImplicitly]
[CliCommand(["backup", "show"], Name = "show", Description = "Show snapshot details")]
public class BackupShowCommand(
    ISnapshotManager snapshotManager,
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
            Console.WriteLine();
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
