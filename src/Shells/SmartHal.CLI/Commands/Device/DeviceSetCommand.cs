using CreativeCoders.Cli.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Adapters;
using SmartHal.Core.Config;
using SmartHal.Core.Devices;

namespace SmartHal.CLI.Commands.Device;

/// <summary>
/// Sets a parameter on a device via the adapter and updates the YAML file.
/// </summary>
[UsedImplicitly]
[CliCommand(["device", "set"], Name = "set", Description = "Set a device parameter")]
public class DeviceSetCommand(
    IConfigRepository configRepository,
    IAdapterFactory adapterFactory,
    OutputFormatter formatter,
    ILogger<DeviceSetCommand> logger) : ICliCommand<DeviceSetOptions>
{
    private readonly ILogger<DeviceSetCommand> _logger = logger;

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(DeviceSetOptions options)
    {
        _logger.LogInformation("Setting parameter {ParameterName} on device {DeviceId}", options.Parameter, options.DeviceId);

        var device = await configRepository.GetDeviceAsync(options.DeviceId).ConfigureAwait(false);
        var adapterConfig = await configRepository.GetAdapterConfigAsync(device.AdapterId).ConfigureAwait(false);

        await using var adapter = adapterFactory.CreateAdapter(adapterConfig);

        if (adapter is not IDeviceWriter writer)
        {
            formatter.WriteError($"Adapter '{device.AdapterId}' does not support writing parameters.");
            return new CommandResult(1);
        }

        var paramValue = ParameterValue.FromString(options.Value);

        await writer.WriteDeviceParameterAsync(device.NativeId, options.Parameter, paramValue).ConfigureAwait(false);

        // Update YAML
        device.Parameters[options.Parameter] = paramValue;
        await configRepository.SaveDeviceAsync(device).ConfigureAwait(false);

        formatter.WriteSuccess($"Set '{options.Parameter}' = '{options.Value}' on device '{options.DeviceId}'.");
        return CommandResult.Success;
    }
}
