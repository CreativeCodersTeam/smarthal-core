using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using ILogger = Serilog.ILogger;

namespace SmartHal.Core.Logging;

/// <summary>
/// Configures Serilog based on CLI verbosity flags and provides a bridge
/// to the <c>Microsoft.Extensions.Logging</c> <see cref="ILoggerFactory"/> abstraction.
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
    /// Registers <see cref="ILoggerFactory"/> and <see cref="Microsoft.Extensions.Logging.ILogger{T}"/>
    /// in the DI container, routing all output through the already-configured Serilog pipeline.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    public static IServiceCollection AddSmartHalLogging(this IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: false);
        });

        return services;
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
