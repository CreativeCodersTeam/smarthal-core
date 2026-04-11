using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
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
    OutputFormatter formatter,
    ILogger<BackupShowCommand> logger) : ICliCommand<BackupShowOptions>
{
    private readonly ISnapshotManager _snapshotManager = Ensure.NotNull(snapshotManager);
    private readonly IAnsiConsole _console = Ensure.NotNull(console);
    private readonly OutputFormatter _formatter = Ensure.NotNull(formatter);
    private readonly ILogger<BackupShowCommand> _logger = Ensure.NotNull(logger);

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(BackupShowOptions options)
    {
        _logger.LogInformation("Showing snapshot {SnapshotId}", options.SnapshotId);

        var manifest = await _snapshotManager.GetSnapshotAsync(options.SnapshotId).ConfigureAwait(false);

        _formatter.WriteObject(
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
            _console.WriteLine();
            _formatter.WriteTable(
                manifest.Entries.ToList(),
                ("Device ID", e => e.DeviceId),
                ("Adapter", e => e.AdapterId),
                ("Native ID", e => e.NativeId),
                ("Config", e => e.ConfigFilePath));
        }

        return CommandResult.Success;
    }
}
