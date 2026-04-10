using CreativeCoders.Cli.Core;
using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Backup;

namespace SmartHal.CLI.Commands.Backup;

/// <summary>Options for the backup restore command.</summary>
public class BackupRestoreOptions
{
    /// <summary>The snapshot ID to restore.</summary>
    [OptionValue(0, HelpText = "The snapshot ID to restore")]
    public string SnapshotId { get; set; } = string.Empty;

    /// <summary>Preview changes without applying.</summary>
    [OptionParameter('d', "dry-run", HelpText = "Preview changes without restoring")]
    public bool DryRun { get; set; }
}

/// <summary>
/// Restores a configuration snapshot.
/// </summary>
[UsedImplicitly]
[CliCommand(["backup", "restore"], Name = "restore", Description = "Restore a snapshot")]
public class BackupRestoreCommand(
    IRestoreOrchestrator orchestrator,
    IUserInteraction interaction,
    OutputFormatter formatter) : ICliCommand<BackupRestoreOptions>
{
    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(BackupRestoreOptions options)
    {
        var preview = await orchestrator.PreviewRestoreAsync(options.SnapshotId).ConfigureAwait(false);

        if (!preview.HasChanges)
        {
            OutputFormatter.WriteSuccess("No changes to restore — snapshot matches current state.");
            return CommandResult.Success;
        }

        // Show preview
        var changedDevices = preview.Devices
            .Where(d => d.Diff.HasChanges)
            .ToList();

        foreach (var device in changedDevices)
        {
            Console.Error.WriteLine($"  Device: {device.DeviceId} {(device.DeviceExists ? "" : "(deleted)")}");
            formatter.WriteTable(
                device.Diff.Changes.ToList(),
                ("Kind", c => c.Kind.ToString()),
                ("Path", c => c.Path),
                ("Current", c => c.OldValue ?? "-"),
                ("Snapshot", c => c.NewValue ?? "-"));
        }

        if (options.DryRun)
        {
            OutputFormatter.WriteSuccess("Dry run — no changes applied.");
            return CommandResult.Success;
        }

        if (!interaction.Confirm("Proceed with restore?"))
        {
            OutputFormatter.WriteSuccess("Cancelled.");
            return CommandResult.Success;
        }

        var result = await orchestrator.RestoreAsync(options.SnapshotId).ConfigureAwait(false);

        OutputFormatter.WriteSuccess(
            $"Restored {result.DevicesRestored} device(s), {result.DevicesSkipped} skipped.");

        if (!result.Success)
        {
            foreach (var error in result.Errors)
            {
                OutputFormatter.WriteError($"  {error.DeviceId}: {error.Message}");
            }
        }

        return result.Success ? CommandResult.Success : new CommandResult(1);
    }
}
