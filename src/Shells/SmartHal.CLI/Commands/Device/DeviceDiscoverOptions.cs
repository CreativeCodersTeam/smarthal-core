using CreativeCoders.SysConsole.Cli.Parsing;

namespace SmartHal.CLI.Commands.Device;

/// <summary>Options for the device discover command.</summary>
public class DeviceDiscoverOptions
{
    /// <summary>The adapter ID to discover devices from.</summary>
    [OptionValue(0, HelpText = "The adapter ID to discover devices from")]
    public string AdapterId { get; set; } = string.Empty;
}