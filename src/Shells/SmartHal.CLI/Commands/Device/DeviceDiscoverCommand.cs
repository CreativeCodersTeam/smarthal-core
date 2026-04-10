using CreativeCoders.Cli.Core;
using JetBrains.Annotations;
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
    OutputFormatter formatter) : ICliCommand<DeviceDiscoverOptions>
{
    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(DeviceDiscoverOptions options)
    {
        var adapterConfig = await configRepository.GetAdapterConfigAsync(options.AdapterId).ConfigureAwait(false);
        await using var adapter = adapterFactory.CreateAdapter(adapterConfig);

        if (adapter is not IDeviceDiscovery discovery)
        {
            formatter.WriteError($"Adapter '{options.AdapterId}' does not support device discovery.");
            return new CommandResult(1);
        }

        var discovered = await discovery.DiscoverDevicesAsync().ConfigureAwait(false);

        if (discovered.Count == 0)
        {
            formatter.WriteSuccess("No new devices discovered.");
            return CommandResult.Success;
        }

        formatter.WriteTable(
            discovered.ToList(),
            ("Native ID", d => d.NativeId),
            ("Name", d => d.Name),
            ("Type", d => d.Type.Value));

        foreach (var device in discovered)
        {
            var answer = interaction.Confirm($"Import '{device.Name}' ({device.NativeId})?");
            if (!answer)
            {
                continue;
            }

            var deviceId = await idGenerator.GenerateDeviceIdAsync(
                adapterConfig.AdapterType, device.Name).ConfigureAwait(false);

            device.Id = deviceId;
            device.AdapterId = options.AdapterId;

            await configRepository.SaveDeviceAsync(device).ConfigureAwait(false);
            formatter.WriteSuccess($"Imported as '{deviceId}'.");
        }

        return CommandResult.Success;
    }
}
