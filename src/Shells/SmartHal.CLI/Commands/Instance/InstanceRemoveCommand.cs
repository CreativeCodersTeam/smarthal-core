using CreativeCoders.Cli.Core;
using JetBrains.Annotations;
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
    OutputFormatter formatter) : ICliCommand<InstanceRemoveOptions>
{
    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(InstanceRemoveOptions options)
    {
        // Verify instance exists
        var config = await configRepository.GetAdapterConfigAsync(options.InstanceId).ConfigureAwait(false);

        if (!interaction.Confirm($"Remove adapter instance '{config.AdapterId}' (type: {config.AdapterType})?"))
        {
            formatter.WriteSuccess("Cancelled.");
            return CommandResult.Success;
        }

        // Delete adapter config file
        var adapterFilePath = Path.Combine("adapters", $"{options.InstanceId}.yaml");
        if (File.Exists(adapterFilePath))
        {
            File.Delete(adapterFilePath);
        }

        formatter.WriteSuccess($"Instance '{options.InstanceId}' removed.");
        return CommandResult.Success;
    }
}
