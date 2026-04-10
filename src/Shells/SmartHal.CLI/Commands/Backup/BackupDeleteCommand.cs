using CreativeCoders.Cli.Core;
using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Backup;

namespace SmartHal.CLI.Commands.Backup;

/// <summary>Options for the backup delete command.</summary>
public class BackupDeleteOptions
{
    /// <summary>The snapshot ID to delete.</summary>
    [OptionValue(0, HelpText = "The snapshot ID to delete")]
    public string SnapshotId { get; set; } = string.Empty;
}

/// <summary>
/// Deletes a snapshot.
/// </summary>
[UsedImplicitly]
[CliCommand(["backup", "delete"], Name = "delete", Description = "Delete a snapshot")]
public class BackupDeleteCommand(
    ISnapshotManager snapshotManager,
    IUserInteraction interaction) : ICliCommand<BackupDeleteOptions>
{
    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(BackupDeleteOptions options)
    {
        if (!interaction.Confirm($"Delete snapshot '{options.SnapshotId}'?"))
        {
            OutputFormatter.WriteSuccess("Cancelled.");
            return CommandResult.Success;
        }

        await snapshotManager.DeleteSnapshotAsync(options.SnapshotId).ConfigureAwait(false);

        OutputFormatter.WriteSuccess($"Snapshot '{options.SnapshotId}' deleted.");
        return CommandResult.Success;
    }
}
