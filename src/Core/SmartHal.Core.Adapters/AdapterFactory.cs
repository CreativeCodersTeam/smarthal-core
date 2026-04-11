using System.Reflection;
using CreativeCoders.Core;
using Microsoft.Extensions.Logging;

namespace SmartHal.Core.Adapters;

/// <summary>
/// Creates adapter instances by scanning registered assemblies for
/// <see cref="AdapterMetadataAttribute"/> and <see cref="ISmartHalAdapter"/> implementations.
/// </summary>
public class AdapterFactory : IAdapterFactory
{
    private readonly ILogger<AdapterFactory> _logger;
    private readonly Dictionary<string, AdapterRegistration> _registrations = [];

    public AdapterFactory(ILogger<AdapterFactory> logger)
    {
        _logger = Ensure.NotNull(logger);
    }

    /// <summary>
    /// Registers an assembly for adapter discovery. Scans all classes that implement
    /// <see cref="ISmartHalAdapter"/> and are decorated with <see cref="AdapterMetadataAttribute"/>.
    /// </summary>
    /// <param name="assembly">The assembly to scan for adapter implementations.</param>
    /// <exception cref="SmartHalAdapterException">An adapter type from this assembly is already registered.</exception>
    public void RegisterAssembly(Assembly assembly)
    {
        Ensure.NotNull(assembly);

        _logger.LogInformation("Scanning assembly {AssemblyName} for adapter implementations", assembly.GetName().Name);

        var registeredCount = 0;

        var adapterTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false }
                        && typeof(ISmartHalAdapter).IsAssignableFrom(t));

        foreach (var adapterType in adapterTypes)
        {
            var metadata = adapterType.GetCustomAttribute<AdapterMetadataAttribute>();
            if (metadata is null)
            {
                continue;
            }

            _logger.LogDebug("Found adapter type {AdapterType} in class {ClassName}", metadata.AdapterType, adapterType.FullName);

            if (_registrations.ContainsKey(metadata.AdapterType))
            {
                _logger.LogWarning("Adapter type {AdapterType} is already registered", metadata.AdapterType);

                throw new SmartHalAdapterException(
                    $"Adapter type '{metadata.AdapterType}' is already registered.",
                    metadata.AdapterType);
            }

            _registrations[metadata.AdapterType] = new AdapterRegistration(metadata, adapterType);
            registeredCount++;
        }

        _logger.LogDebug("Registered {Count} adapter type(s) from assembly {AssemblyName}", registeredCount, assembly.GetName().Name);
    }

    /// <inheritdoc />
    public ISmartHalAdapter CreateAdapter(AdapterConfig config)
    {
        Ensure.NotNull(config);

        _logger.LogDebug("Creating adapter instance for type {AdapterType} (adapterId: {AdapterId})", config.AdapterType, config.AdapterId);

        if (!_registrations.TryGetValue(config.AdapterType, out var registration))
        {
            _logger.LogWarning("Unknown adapter type {AdapterType}", config.AdapterType);

            throw new SmartHalAdapterException(
                $"Unknown adapter type: '{config.AdapterType}'.",
                config.AdapterId);
        }

        var instance = Activator.CreateInstance(registration.ImplementationType)
                       ?? throw new SmartHalAdapterException(
                           $"Failed to create instance of adapter type '{config.AdapterType}'.",
                           config.AdapterId);

        _logger.LogInformation("Created adapter instance {AdapterType} for {AdapterId}", config.AdapterType, config.AdapterId);

        return (ISmartHalAdapter)instance;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetAvailableAdapterTypes() =>
        _registrations.Keys.ToList();

    private sealed record AdapterRegistration(AdapterMetadataAttribute Metadata, Type ImplementationType);
}
