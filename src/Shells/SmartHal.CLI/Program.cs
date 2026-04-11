using CreativeCoders.Cli.Hosting;
using CreativeCoders.Cli.Hosting.Help;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using SmartHal.CLI.Infrastructure;
using SmartHal.Core;
using SmartHal.Core.Adapters;
using SmartHal.Core.Backup;
using SmartHal.Core.Config;
using SmartHal.Core.Logging;
using SmartHal.Core.Secrets;
using Spectre.Console;

// Parse global options before building the host
var cliContext = new CliContext();
var commandArgs = cliContext.ParseGlobalOptions(args);

// Configure logging (logs go to stderr)
Log.Logger = LogSetup.Configure(cliContext.Verbosity);

try
{
    var host = CliHostBuilder.Create()
        .EnableHelp(HelpCommandKind.CommandOrArgument)
        .ConfigureServices(services =>
        {
            // Logging bridge: ILogger<T> → Serilog
            services.AddSmartHalLogging();

            // Global state
            services.AddSingleton(cliContext);
            services.AddSingleton<OutputFormatter>();
            services.AddSingleton<IUserInteraction, ConsoleUserInteraction>();

            // Core services (stateless singletons)
            services.AddSingleton<IConfigReader, YamlConfigReader>();
            services.AddSingleton<IConfigWriter, YamlConfigWriter>();
            services.AddSingleton<IConfigValidator, ConfigValidator>();
            services.AddSingleton<IDeviceEnricher, DeviceEnricher>();
            services.AddSingleton<IConfigDiffer, ConfigDiffer>();
            services.AddSingleton<IConfigApplier, ConfigApplier>();
            services.AddSingleton<SecretsProviderFactory>(sp =>
                new SecretsProviderFactory(
                    sp.GetRequiredService<ILogger<SecretsProviderFactory>>(),
                    () => Task.FromResult(
                        sp.GetRequiredService<IUserInteraction>().ReadSecret("Enter password: "))));

            // ConfigRepository — scoped to the configured path
            services.AddSingleton<IConfigRepository>(sp =>
                new FileConfigRepository(
                    cliContext.ConfigPath,
                    sp.GetRequiredService<IConfigReader>(),
                    sp.GetRequiredService<IConfigWriter>(),
                    sp.GetRequiredService<ILogger<FileConfigRepository>>()));

            // ID generation
            services.AddSingleton<IIdGenerator>(sp =>
                new DeviceIdGenerator(
                    cliContext.ConfigPath,
                    sp.GetRequiredService<ILogger<DeviceIdGenerator>>()));

            // Adapter factory with HomeMatic registered
            services.AddSingleton<IAdapterFactory>(sp =>
            {
                var factory = new AdapterFactory(
                    sp.GetRequiredService<ILogger<AdapterFactory>>());
                factory.RegisterAssembly(SmartHal.Adapters.HomeMatic.AssemblyReference.Assembly);
                return factory;
            });

            // Backup services
            services.AddSingleton<ISnapshotManager>(sp =>
                new SnapshotManager(
                    Path.Combine(cliContext.ConfigPath, "snapshots"),
                    sp.GetRequiredService<IConfigRepository>(),
                    logger: sp.GetRequiredService<ILogger<SnapshotManager>>()));

            services.AddSingleton<IRestoreOrchestrator>(sp =>
                new RestoreOrchestrator(
                    Path.Combine(cliContext.ConfigPath, "snapshots"),
                    sp.GetRequiredService<ISnapshotManager>(),
                    sp.GetRequiredService<IConfigRepository>(),
                    sp.GetRequiredService<IConfigReader>(),
                    sp.GetRequiredService<IConfigDiffer>(),
                    sp.GetRequiredService<ILogger<RestoreOrchestrator>>()));
        })
        .Build();

    return await host.RunMainAsync(commandArgs).ConfigureAwait(false);
}
catch (Exception ex) when (ex is SmartHalException)
{
    AnsiConsole.MarkupLine($"[red]Error: {Markup.Escape(ex.Message)}[/]");
    Log.Debug(ex, "SmartHal error");
    return ExitCodes.FromException(ex);
}
catch (Exception ex)
{
    AnsiConsole.MarkupLine($"[red]Error: {Markup.Escape(ex.Message)}[/]");
    Log.Error(ex, "Unexpected error");
    return ExitCodes.GeneralError;
}
finally
{
    await Log.CloseAndFlushAsync().ConfigureAwait(false);
}
