using SmartHal.Core.Logging;

namespace SmartHal.CLI.Infrastructure;

/// <summary>
/// Holds global CLI state parsed from command-line arguments before command dispatch.
/// Registered as a singleton and available for injection into all commands.
/// </summary>
public class CliContext
{
    /// <summary>Path to the SmartHal configuration directory.</summary>
    public string ConfigPath { get; set; } = GetDefaultConfigPath();

    /// <summary>The desired log verbosity level.</summary>
    public LogVerbosity Verbosity { get; set; } = LogVerbosity.Normal;

    /// <summary>The output format for command results.</summary>
    public OutputFormat OutputFormat { get; set; } = OutputFormat.Table;

    /// <summary>
    /// Parses global options from the raw command-line arguments and returns
    /// the remaining arguments for command dispatch.
    /// </summary>
    /// <param name="args">The raw command-line arguments.</param>
    /// <returns>The arguments with global options removed.</returns>
    public string[] ParseGlobalOptions(string[] args)
    {
        var remaining = new List<string>();

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--config-path" when i + 1 < args.Length:
                    ConfigPath = args[++i];
                    break;
                case "-vv":
                    Verbosity = LogVerbosity.Verbose;
                    break;
                case "-v" or "--verbose":
                    Verbosity = LogVerbosity.Debug;
                    break;
                case "--quiet":
                    Verbosity = LogVerbosity.Quiet;
                    break;
                case "--output" when i + 1 < args.Length:
                    OutputFormat = ParseOutputFormat(args[++i]);
                    break;
                default:
                    remaining.Add(args[i]);
                    break;
            }
        }

        return remaining.ToArray();
    }

    private static string GetDefaultConfigPath() =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".smarthal");

    private static OutputFormat ParseOutputFormat(string value) => value.ToLowerInvariant() switch
    {
        "json" => OutputFormat.Json,
        "yaml" => OutputFormat.Yaml,
        "table" => OutputFormat.Table,
        _ => OutputFormat.Table
    };
}
