using Serilog;
using Serilog.Events;

namespace SmartHal.Core.Logging;

/// <summary>
/// Configures Serilog based on CLI verbosity flags.
/// Results go to stdout, logs go to stderr.
/// </summary>
public static class LogSetup
{
    private const string OutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
    private const int RetainedFileCountLimit = 7;

    /// <summary>
    /// Creates and returns a configured Serilog logger.
    /// </summary>
    /// <param name="verbosity">The desired verbosity level.</param>
    /// <param name="logFilePath">Optional file path for file-based logging with daily rolling.</param>
    /// <returns>A configured <see cref="ILogger"/> instance.</returns>
    public static ILogger Configure(LogVerbosity verbosity, string? logFilePath = null)
    {
        var minimumLevel = MapVerbosityToLevel(verbosity);

        var config = new LoggerConfiguration()
            .MinimumLevel.Is(minimumLevel)
            .WriteTo.Console(
                restrictedToMinimumLevel: minimumLevel,
                outputTemplate: OutputTemplate,
                standardErrorFromLevel: LogEventLevel.Verbose);

        if (logFilePath is not null)
        {
            config.WriteTo.File(
                logFilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: RetainedFileCountLimit,
                outputTemplate: OutputTemplate);
        }

        return config.CreateLogger();
    }

    /// <summary>
    /// Maps a <see cref="LogVerbosity"/> value to the corresponding Serilog <see cref="LogEventLevel"/>.
    /// </summary>
    /// <param name="verbosity">The verbosity level to map.</param>
    /// <returns>The corresponding <see cref="LogEventLevel"/>.</returns>
    internal static LogEventLevel MapVerbosityToLevel(LogVerbosity verbosity) => verbosity switch
    {
        LogVerbosity.Quiet => LogEventLevel.Warning,
        LogVerbosity.Normal => LogEventLevel.Information,
        LogVerbosity.Debug => LogEventLevel.Debug,
        LogVerbosity.Verbose => LogEventLevel.Verbose,
        _ => LogEventLevel.Information
    };
}
