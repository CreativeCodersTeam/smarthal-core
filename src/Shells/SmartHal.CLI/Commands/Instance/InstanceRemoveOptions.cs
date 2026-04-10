using CreativeCoders.SysConsole.Cli.Parsing;

namespace SmartHal.CLI.Commands.Instance;

/// <summary>Options for the instance remove command.</summary>
public class InstanceRemoveOptions
{
    /// <summary>The instance ID to remove.</summary>
    [OptionValue(0, HelpText = "The instance ID to remove")]
    public string InstanceId { get; set; } = string.Empty;
}