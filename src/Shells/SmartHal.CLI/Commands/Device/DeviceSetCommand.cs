using CreativeCoders.Cli.Core;
using CreativeCoders.Core;
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
    private readonly IConfigRepository _configRepository = Ensure.NotNull(configRepository);
    private readonly IAdapterFactory _adapterFactory = Ensure.NotNull(adapterFactory);
    private readonly OutputFormatter _formatter = Ensure.NotNull(formatter);
    private readonly ILogger<DeviceSetCommand> _logger = Ensure.NotNull(logger);

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(DeviceSetOptions options)
    {
        _logger.LogInformation("Setting parameter {ParameterName} on device {DeviceId}", options.Parameter, options.DeviceId);

        var device = await _configRepository.GetDeviceAsync(options.DeviceId).ConfigureAwait(false);
        var adapterConfig = await _configRepository.GetAdapterConfigAsync(device.AdapterId).ConfigureAwait(false);

        await using var adapter = _adapterFactory.CreateAdapter(adapterConfig);

        if (adapter is not IDeviceWriter writer)
        {
            _formatter.WriteError($"Adapter '{device.AdapterId}' does not support writing parameters.");
            return new CommandResult(1);
        }

        var paramValue = ParameterValue.FromString(options.Value);

        await writer.WriteDeviceParameterAsync(device.NativeId, options.Parameter, paramValue).ConfigureAwait(false);

        // Update YAML
        device.Parameters[options.Parameter] = paramValue;
        await _configRepository.SaveDeviceAsync(device).ConfigureAwait(false);

        _formatter.WriteSuccess($"Set '{options.Parameter}' = '{options.Value}' on device '{options.DeviceId}'.");
        return CommandResult.Success;
    }
}
