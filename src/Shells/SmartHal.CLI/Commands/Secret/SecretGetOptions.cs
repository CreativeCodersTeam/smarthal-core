using CreativeCoders.SysConsole.Cli.Parsing;

namespace SmartHal.CLI.Commands.Secret;

/// <summary>Options for the secret get command.</summary>
public class SecretGetOptions
{
    /// <summary>The secret key to retrieve.</summary>
    [OptionValue(0, HelpText = "The secret key")]
    public string Key { get; set; } = string.Empty;
}