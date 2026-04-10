using CreativeCoders.Cli.Core;
using CreativeCoders.SysConsole.Cli.Parsing;
using JetBrains.Annotations;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Adapters;
using SmartHal.Core.Config;

namespace SmartHal.CLI.Commands.Adapter;

/// <summary>Options for the adapter configure command.</summary>
public class AdapterConfigureOptions
{
    /// <summary>The adapter ID to configure.</summary>
    [OptionValue(0, HelpText = "The adapter ID to configure")]
    public string AdapterId { get; set; } = string.Empty;
}

/// <summary>
/// Interactively configures an adapter instance.
/// </summary>
[UsedImplicitly]
[CliCommand(["adapter", "configure"], Name = "configure", Description = "Configure an adapter instance")]
public class AdapterConfigureCommand(
    IConfigRepository configRepository,
    IUserInteraction interaction) : ICliCommand<AdapterConfigureOptions>
{
    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(AdapterConfigureOptions options)
    {
        AdapterConfig config;

        try
        {
            config = await configRepository.GetAdapterConfigAsync(options.AdapterId).ConfigureAwait(false);
        }
        catch
        {
            OutputFormatter.WriteError($"Adapter '{options.AdapterId}' not found.");
            return new CommandResult(1);
        }

        Console.Error.WriteLine($"Configuring adapter '{config.AdapterId}' (type: {config.AdapterType})");
        Console.Error.WriteLine("Enter settings (empty value to skip):");

        var updated = false;
        foreach (var key in config.Settings.Keys.ToList())
        {
            var current = config.Settings[key];
            var isSecret = key.EndsWith("_key", StringComparison.OrdinalIgnoreCase);
            var prompt = $"  {key} [{(isSecret ? "***" : current)}]: ";

            var value = isSecret
                ? interaction.ReadSecret(prompt)
                : interaction.ReadLine(prompt);

            if (!string.IsNullOrEmpty(value))
            {
                config.Settings[key] = value;
                updated = true;
            }
        }

        if (updated)
        {
            await configRepository.SaveAdapterConfigAsync(config).ConfigureAwait(false);
            OutputFormatter.WriteSuccess($"Adapter '{options.AdapterId}' configuration updated.");
        }
        else
        {
            OutputFormatter.WriteSuccess("No changes made.");
        }

        return CommandResult.Success;
    }
}
