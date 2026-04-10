using CreativeCoders.SysConsole.Cli.Parsing;

namespace SmartHal.CLI.Commands.Instance;

/// <summary>Options for the instance add command.</summary>
public class InstanceAddOptions
{
    /// <summary>The adapter type.</summary>
    [OptionValue(0, HelpText = "The adapter type (e.g. homematic)")]
    public string AdapterType { get; set; } = string.Empty;

    /// <summary>The instance ID to create.</summary>
    [OptionValue(1, HelpText = "The instance ID")]
    public string InstanceId { get; set; } = string.Empty;
}