using CreativeCoders.Cli.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core.Config;

namespace SmartHal.CLI.Commands.Device;

/// <summary>
/// Shows all details of a specific device.
/// </summary>
[UsedImplicitly]
[CliCommand(["device", "show"], Name = "show", Description = "Show device details")]
public class DeviceShowCommand(
    IConfigRepository configRepository,
    OutputFormatter formatter,
    ILogger<DeviceShowCommand> logger) : ICliCommand<DeviceShowOptions>
{
    private readonly ILogger<DeviceShowCommand> _logger = logger;

    /// <inheritdoc />
    public async Task<CommandResult> ExecuteAsync(DeviceShowOptions options)
    {
        _logger.LogInformation("Showing device {DeviceId}", options.DeviceId);

        var device = await configRepository.GetDeviceAsync(options.DeviceId).ConfigureAwait(false);

        formatter.WriteObject(
            device,
            ("ID", device.Id),
            ("Name", device.Name),
            ("Type", device.Type.Value),
            ("Adapter", device.AdapterId),
            ("Native ID", device.NativeId),
            ("Room", device.RoomId ?? "-"),
            ("Groups", device.GroupIds.Count > 0 ? string.Join(", ", device.GroupIds) : "-"),
            ("Parameters", device.Parameters.Count.ToString()),
            ("Channels", device.Channels.Count.ToString()),
            ("Relations", device.Relations.Count.ToString()));

        return CommandResult.Success;
    }
}
