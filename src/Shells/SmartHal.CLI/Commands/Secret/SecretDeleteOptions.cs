using CreativeCoders.SysConsole.Cli.Parsing;

namespace SmartHal.CLI.Commands.Secret;

/// <summary>Options for the secret delete command.</summary>
public class SecretDeleteOptions
{
    /// <summary>The secret key to delete.</summary>
    [OptionValue(0, HelpText = "The secret key to delete")]
    public string Key { get; set; } = string.Empty;
}