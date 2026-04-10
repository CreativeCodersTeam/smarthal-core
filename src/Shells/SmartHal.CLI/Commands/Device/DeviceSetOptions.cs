using CreativeCoders.SysConsole.Cli.Parsing;

namespace SmartHal.CLI.Commands.Device;

/// <summary>Options for the device set command.</summary>
public class DeviceSetOptions
{
    /// <summary>The device ID.</summary>
    [OptionValue(0, HelpText = "The device ID")]
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>The parameter name to set.</summary>
    [OptionValue(1, HelpText = "The parameter name")]
    public string Parameter { get; set; } = string.Empty;

    /// <summary>The value to set.</summary>
    [OptionValue(2, HelpText = "The value to set")]
    public string Value { get; set; } = string.Empty;
}