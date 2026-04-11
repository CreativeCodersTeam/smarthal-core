using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
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
    private readonly ISnapshotManager _snapshotManager = Ensure.NotNull(snapshotManager);
    private readonly IUserInteraction _interaction = Ensure.NotNull(interaction);
    private readonly OutputFormatter _formatter = Ensure.NotNull(formatter);
    private readonly ILogger<BackupDeleteCommand> _logger = Ensure.NotNull(logger);

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(BackupDeleteOptions options)
    {
        _logger.LogInformation("Deleting snapshot {SnapshotId}", options.SnapshotId);

        if (!_interaction.Confirm($"Delete snapshot '{options.SnapshotId}'?"))
        {
            _formatter.WriteSuccess("Cancelled.");
            return CommandResult.Success;
        }

        await _snapshotManager.DeleteSnapshotAsync(options.SnapshotId).ConfigureAwait(false);

        _formatter.WriteSuccess($"Snapshot '{options.SnapshotId}' deleted.");
        return CommandResult.Success;
    }
}
