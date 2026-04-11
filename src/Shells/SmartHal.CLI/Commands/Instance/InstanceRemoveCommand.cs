using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Config;

namespace SmartHal.CLI.Commands.Instance;

/// <summary>
/// Removes an adapter instance after confirmation.
/// </summary>
[UsedImplicitly]
[CliCommand(["instance", "remove"], Name = "remove", Description = "Remove an adapter instance")]
public class InstanceRemoveCommand(
    IConfigRepository configRepository,
    IUserInteraction interaction,
    OutputFormatter formatter,
    ILogger<InstanceRemoveCommand> logger) : ICliCommand<InstanceRemoveOptions>
{
    private readonly IConfigRepository _configRepository = Ensure.NotNull(configRepository);
    private readonly IUserInteraction _interaction = Ensure.NotNull(interaction);
    private readonly OutputFormatter _formatter = Ensure.NotNull(formatter);
    private readonly ILogger<InstanceRemoveCommand> _logger = Ensure.NotNull(logger);

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(InstanceRemoveOptions options)
    {
        _logger.LogInformation("Removing adapter instance {InstanceId}", options.InstanceId);

        // Verify instance exists
        var config = await _configRepository.GetAdapterConfigAsync(options.InstanceId).ConfigureAwait(false);

        if (!_interaction.Confirm($"Remove adapter instance '{config.AdapterId}' (type: {config.AdapterType})?"))
        {
            _formatter.WriteSuccess("Cancelled.");
            return CommandResult.Success;
        }

        // Delete adapter config file
        var adapterFilePath = Path.Combine("adapters", $"{options.InstanceId}.yaml");
        if (File.Exists(adapterFilePath))
        {
            File.Delete(adapterFilePath);
        }

        _formatter.WriteSuccess($"Instance '{options.InstanceId}' removed.");
        return CommandResult.Success;
    }
}
