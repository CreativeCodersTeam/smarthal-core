using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Adapters;
using SmartHal.Core.Config;

namespace SmartHal.CLI.Commands.Device;

/// <summary>
/// Discovers devices via the specified adapter and offers to import them.
/// </summary>
[UsedImplicitly]
[CliCommand(["device", "discover"], Name = "discover", Description = "Discover devices via adapter")]
public class DeviceDiscoverCommand(
    IConfigRepository configRepository,
    IAdapterFactory adapterFactory,
    IIdGenerator idGenerator,
    IUserInteraction interaction,
    OutputFormatter formatter,
    ILogger<DeviceDiscoverCommand> logger) : ICliCommand<DeviceDiscoverOptions>
{
    private readonly IConfigRepository _configRepository = Ensure.NotNull(configRepository);
    private readonly IAdapterFactory _adapterFactory = Ensure.NotNull(adapterFactory);
    private readonly IIdGenerator _idGenerator = Ensure.NotNull(idGenerator);
    private readonly IUserInteraction _interaction = Ensure.NotNull(interaction);
    private readonly OutputFormatter _formatter = Ensure.NotNull(formatter);
    private readonly ILogger<DeviceDiscoverCommand> _logger = Ensure.NotNull(logger);

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(DeviceDiscoverOptions options)
    {
        _logger.LogInformation("Discovering devices via adapter {AdapterId}", options.AdapterId);

        var adapterConfig = await _configRepository.GetAdapterConfigAsync(options.AdapterId).ConfigureAwait(false);
        await using var adapter = _adapterFactory.CreateAdapter(adapterConfig);

        if (adapter is not IDeviceDiscovery discovery)
        {
            _formatter.WriteError($"Adapter '{options.AdapterId}' does not support device discovery.");
            return new CommandResult(1);
        }

        var discovered = await discovery.DiscoverDevicesAsync().ConfigureAwait(false);

        if (discovered.Count == 0)
        {
            _formatter.WriteSuccess("No new devices discovered.");
            return CommandResult.Success;
        }

        _formatter.WriteTable(
            discovered.ToList(),
            ("Native ID", d => d.NativeId),
            ("Name", d => d.Name),
            ("Type", d => d.Type.Value));

        foreach (var device in discovered)
        {
            var answer = _interaction.Confirm($"Import '{device.Name}' ({device.NativeId})?");
            if (!answer)
            {
                continue;
            }

            var deviceId = await _idGenerator.GenerateDeviceIdAsync(
                adapterConfig.AdapterType, device.Name).ConfigureAwait(false);

            device.Id = deviceId;
            device.AdapterId = options.AdapterId;

            await _configRepository.SaveDeviceAsync(device).ConfigureAwait(false);
            _formatter.WriteSuccess($"Imported as '{deviceId}'.");
        }

        return CommandResult.Success;
    }
}
