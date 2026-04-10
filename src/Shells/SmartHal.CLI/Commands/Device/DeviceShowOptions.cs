using CreativeCoders.SysConsole.Cli.Parsing;

namespace SmartHal.CLI.Commands.Device;

/// <summary>Options for the device show command.</summary>
public class DeviceShowOptions
{
    /// <summary>The device ID to display.</summary>
    [OptionValue(0, HelpText = "The device ID to display")]
    public string DeviceId { get; set; } = string.Empty;
}