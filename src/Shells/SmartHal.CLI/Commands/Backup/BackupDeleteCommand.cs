using CreativeCoders.Cli.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Backup;

namespace SmartHal.CLI.Commands.Backup;

/// <summary>
/// Deletes a snapshot.
/// </summary>
[UsedImplicitly]
[CliCommand(["backup", "delete"], Name = "delete", Description = "Delete a snapshot")]
public class BackupDeleteCommand(
    ISnapshotManager snapshotManager,
    IUserInteraction interaction,
    OutputFormatter formatter,
    ILogger<BackupDeleteCommand> logger) : ICliCommand<BackupDeleteOptions>
{
    private readonly ILogger<BackupDeleteCommand> _logger = logger;

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(BackupDeleteOptions options)
    {
        _logger.LogInformation("Deleting snapshot {SnapshotId}", options.SnapshotId);

        if (!interaction.Confirm($"Delete snapshot '{options.SnapshotId}'?"))
        {
            formatter.WriteSuccess("Cancelled.");
            return CommandResult.Success;
        }

        await snapshotManager.DeleteSnapshotAsync(options.SnapshotId).ConfigureAwait(false);

        formatter.WriteSuccess($"Snapshot '{options.SnapshotId}' deleted.");
        return CommandResult.Success;
    }
}
