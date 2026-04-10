using CreativeCoders.Cli.Core;
using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Config;

namespace SmartHal.CLI.Commands.Instance;

/// <summary>Options for the instance remove command.</summary>
public class InstanceRemoveOptions
{
    /// <summary>The instance ID to remove.</summary>
    [OptionValue(0, HelpText = "The instance ID to remove")]
    public string InstanceId { get; set; } = string.Empty;
}

/// <summary>
/// Removes an adapter instance after confirmation.
/// </summary>
[UsedImplicitly]
[CliCommand(["instance", "remove"], Name = "remove", Description = "Remove an adapter instance")]
public class InstanceRemoveCommand(
    IConfigRepository configRepository,
    IUserInteraction interaction) : ICliCommand<InstanceRemoveOptions>
{
    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(InstanceRemoveOptions options)
    {
        // Verify instance exists
        var config = await configRepository.GetAdapterConfigAsync(options.InstanceId).ConfigureAwait(false);

        if (!interaction.Confirm($"Remove adapter instance '{config.AdapterId}' (type: {config.AdapterType})?"))
        {
            OutputFormatter.WriteSuccess("Cancelled.");
            return CommandResult.Success;
        }

        // Delete adapter config file
        var adapterFilePath = Path.Combine("adapters", $"{options.InstanceId}.yaml");
        if (File.Exists(adapterFilePath))
        {
            File.Delete(adapterFilePath);
        }

        OutputFormatter.WriteSuccess($"Instance '{options.InstanceId}' removed.");
        return CommandResult.Success;
    }
}
