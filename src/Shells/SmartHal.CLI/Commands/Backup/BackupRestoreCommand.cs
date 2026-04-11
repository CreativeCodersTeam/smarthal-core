using CreativeCoders.Cli.Core;
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
    private readonly ILogger<BackupRestoreCommand> _logger = logger;

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(BackupRestoreOptions options)
    {
        _logger.LogInformation("Restoring snapshot {SnapshotId} (dryRun: {DryRun})", options.SnapshotId, options.DryRun);

        var preview = await orchestrator.PreviewRestoreAsync(options.SnapshotId).ConfigureAwait(false);

        if (!preview.HasChanges)
        {
            formatter.WriteSuccess("No changes to restore — snapshot matches current state.");
            return CommandResult.Success;
        }

        // Show preview
        var changedDevices = preview.Devices
            .Where(d => d.Diff.HasChanges)
            .ToList();

        foreach (var device in changedDevices)
        {
            console.MarkupLine(
                $"  Device: [bold]{Markup.Escape(device.DeviceId)}[/] {(device.DeviceExists ? "" : "[dim](deleted)[/]")}");
            formatter.WriteTable(
                device.Diff.Changes.ToList(),
                ("Kind", c => c.Kind.ToString()),
                ("Path", c => c.Path),
                ("Current", c => c.OldValue ?? "-"),
                ("Snapshot", c => c.NewValue ?? "-"));
        }

        if (options.DryRun)
        {
            formatter.WriteSuccess("Dry run — no changes applied.");
            return CommandResult.Success;
        }

        if (!interaction.Confirm("Proceed with restore?"))
        {
            formatter.WriteSuccess("Cancelled.");
            return CommandResult.Success;
        }

        var result = await orchestrator.RestoreAsync(options.SnapshotId).ConfigureAwait(false);

        formatter.WriteSuccess(
            $"Restored {result.DevicesRestored} device(s), {result.DevicesSkipped} skipped.");

        if (!result.Success)
        {
            foreach (var error in result.Errors)
            {
                formatter.WriteError($"  {error.DeviceId}: {error.Message}");
            }
        }

        return result.Success ? CommandResult.Success : new CommandResult(1);
    }
}
