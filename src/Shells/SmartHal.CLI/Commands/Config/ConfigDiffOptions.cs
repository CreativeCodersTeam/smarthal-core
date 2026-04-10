using CreativeCoders.SysConsole.Cli.Parsing;

namespace SmartHal.CLI.Commands.Config;

/// <summary>Options for the config diff command.</summary>
public class ConfigDiffOptions
{
    /// <summary>The device ID to compare.</summary>
    [OptionValue(0, HelpText = "The device ID to compare")]
    public string DeviceId { get; set; } = string.Empty;
}