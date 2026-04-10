using CreativeCoders.Cli.Core;
using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Adapters;
using SmartHal.Core.Config;

namespace SmartHal.CLI.Commands.Instance;

/// <summary>Options for the instance add command.</summary>
public class InstanceAddOptions
{
    /// <summary>The adapter type.</summary>
    [OptionValue(0, HelpText = "The adapter type (e.g. homematic)")]
    public string AdapterType { get; set; } = string.Empty;

    /// <summary>The instance ID to create.</summary>
    [OptionValue(1, HelpText = "The instance ID")]
    public string InstanceId { get; set; } = string.Empty;
}

/// <summary>
/// Creates a new adapter instance and starts interactive configuration.
/// </summary>
[UsedImplicitly]
[CliCommand(["instance", "add"], Name = "add", Description = "Add a new adapter instance")]
public class InstanceAddCommand(
    IConfigRepository configRepository,
    IAdapterFactory adapterFactory,
    IUserInteraction interaction) : ICliCommand<InstanceAddOptions>
{
    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(InstanceAddOptions options)
    {
        var availableTypes = adapterFactory.GetAvailableAdapterTypes();
        if (!availableTypes.Contains(options.AdapterType))
        {
            OutputFormatter.WriteError(
                $"Unknown adapter type '{options.AdapterType}'. Available: {string.Join(", ", availableTypes)}");
            return new CommandResult(1);
        }

        var config = new AdapterConfig
        {
            AdapterId = options.InstanceId,
            AdapterType = options.AdapterType,
            Settings = new Dictionary<string, string>()
        };

        Console.Error.WriteLine($"Configuring new instance '{options.InstanceId}' (type: {options.AdapterType})");

        // Collect basic settings interactively
        while (true)
        {
            var key = interaction.ReadLine("  Setting key (empty to finish): ")?.Trim();
            if (string.IsNullOrEmpty(key)) break;

            var isSecret = key.EndsWith("_key", StringComparison.OrdinalIgnoreCase);
            var value = isSecret
                ? interaction.ReadSecret($"  {key} (secret): ")
                : interaction.ReadLine($"  {key}: ");

            if (!string.IsNullOrEmpty(value))
            {
                config.Settings[key] = value;
            }
        }

        await configRepository.SaveAdapterConfigAsync(config).ConfigureAwait(false);

        OutputFormatter.WriteSuccess($"Instance '{options.InstanceId}' created.");
        return CommandResult.Success;
    }
}
