using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Backup;
using Spectre.Console;

namespace SmartHal.CLI.Commands.Backup;

/// <summary>
/// Restores a configuration snapshot.
/// </summary>
[UsedImplicitly]
[CliCommand(["backup", "restore"], Name = "restore", Description = "Restore a snapshot")]
public class BackupRestoreCommand(
    IRestoreOrchestrator orchestrator,
    IUserInteraction interaction,
    IAnsiConsole console,
    OutputFormatter formatter,
    ILogger<BackupRestoreCommand> logger) : ICliCommand<BackupRestoreOptions>
{
    private readonly IRestoreOrchestrator _orchestrator = Ensure.NotNull(orchestrator);
    private readonly IUserInteraction _interaction = Ensure.NotNull(interaction);
    private readonly IAnsiConsole _console = Ensure.NotNull(console);
    private readonly OutputFormatter _formatter = Ensure.NotNull(formatter);
    private readonly ILogger<BackupRestoreCommand> _logger = Ensure.NotNull(logger);

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(BackupRestoreOptions options)
    {
        _logger.LogInformation("Restoring snapshot {SnapshotId} (dryRun: {DryRun})", options.SnapshotId, options.DryRun);

        var preview = await _orchestrator.PreviewRestoreAsync(options.SnapshotId).ConfigureAwait(false);

        if (!preview.HasChanges)
        {
            _formatter.WriteSuccess("No changes to restore — snapshot matches current state.");
            return CommandResult.Success;
        }

        // Show preview
        var changedDevices = preview.Devices
            .Where(d => d.Diff.HasChanges)
            .ToList();

        foreach (var device in changedDevices)
        {
            _console.MarkupLine(
                $"  Device: [bold]{Markup.Escape(device.DeviceId)}[/] {(device.DeviceExists ? "" : "[dim](deleted)[/]")}");
            _formatter.WriteTable(
                device.Diff.Changes.ToList(),
                ("Kind", c => c.Kind.ToString()),
                ("Path", c => c.Path),
                ("Current", c => c.OldValue ?? "-"),
                ("Snapshot", c => c.NewValue ?? "-"));
        }

        if (options.DryRun)
        {
            _formatter.WriteSuccess("Dry run — no changes applied.");
            return CommandResult.Success;
        }

        if (!_interaction.Confirm("Proceed with restore?"))
        {
            _formatter.WriteSuccess("Cancelled.");
            return CommandResult.Success;
        }

        var result = await _orchestrator.RestoreAsync(options.SnapshotId).ConfigureAwait(false);

        _formatter.WriteSuccess(
            $"Restored {result.DevicesRestored} device(s), {result.DevicesSkipped} skipped.");

        if (!result.Success)
        {
            foreach (var error in result.Errors)
            {
                _formatter.WriteError($"  {error.DeviceId}: {error.Message}");
            }
        }

        return result.Success ? CommandResult.Success : new CommandResult(1);
    }
}
