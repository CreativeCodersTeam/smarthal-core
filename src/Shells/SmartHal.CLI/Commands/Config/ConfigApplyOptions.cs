using CreativeCoders.SysConsole.Cli.Parsing;

namespace SmartHal.CLI.Commands.Config;

/// <summary>Options for the config apply command.</summary>
public class ConfigApplyOptions
{
    /// <summary>The device ID to apply configuration to.</summary>
    [OptionValue(0, HelpText = "The device ID to apply configuration to")]
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>Preview changes without applying.</summary>
    [OptionParameter('d', "dry-run", HelpText = "Preview changes without applying")]
    public bool DryRun { get; set; }
}