using System.Reflection;
using CreativeCoders.Core;

namespace SmartHal.Core.Adapters;

/// <summary>
/// Creates adapter instances by scanning registered assemblies for
/// <see cref="AdapterMetadataAttribute"/> and <see cref="ISmartHalAdapter"/> implementations.
/// </summary>
public class AdapterFactory : IAdapterFactory
{
    private readonly Dictionary<string, AdapterRegistration> _registrations = [];

    /// <summary>
    /// Registers an assembly for adapter discovery. Scans all classes that implement
    /// <see cref="ISmartHalAdapter"/> and are decorated with <see cref="AdapterMetadataAttribute"/>.
    /// </summary>
    public void RegisterAssembly(Assembly assembly)
    {
        Ensure.NotNull(assembly);

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

            if (_registrations.ContainsKey(metadata.AdapterType))
            {
                throw new SmartHalAdapterException(
                    $"Adapter type '{metadata.AdapterType}' is already registered.",
                    metadata.AdapterType);
            }

            _registrations[metadata.AdapterType] = new AdapterRegistration(metadata, adapterType);
        }
    }

    /// <inheritdoc />
    public ISmartHalAdapter CreateAdapter(AdapterConfig config)
    {
        Ensure.NotNull(config);

        if (!_registrations.TryGetValue(config.AdapterType, out var registration))
        {
            throw new SmartHalAdapterException(
                $"Unknown adapter type: '{config.AdapterType}'.",
                config.AdapterId);
        }

        var instance = Activator.CreateInstance(registration.ImplementationType)
                       ?? throw new SmartHalAdapterException(
                           $"Failed to create instance of adapter type '{config.AdapterType}'.",
                           config.AdapterId);

        return (ISmartHalAdapter)instance;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetAvailableAdapterTypes() =>
        _registrations.Keys.ToList();

    private sealed record AdapterRegistration(AdapterMetadataAttribute Metadata, Type ImplementationType);
}
