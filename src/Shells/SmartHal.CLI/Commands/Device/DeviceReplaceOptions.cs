using CreativeCoders.SysConsole.Cli.Parsing;

namespace SmartHal.CLI.Commands.Device;

/// <summary>Options for the device replace command.</summary>
public class DeviceReplaceOptions
{
    /// <summary>The device ID to replace.</summary>
    [OptionValue(0, HelpText = "The device ID to replace")]
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>The new native ID for the replacement device.</summary>
    [OptionValue(1, HelpText = "The new native ID")]
    public string NewNativeId { get; set; } = string.Empty;
}