namespace SmartHal.Core.Adapters;

/// <summary>
/// Factory for creating adapter instances by their configured type.
/// </summary>
public interface IAdapterFactory
{
    /// <summary>Creates an adapter instance from the given configuration.</summary>
    /// <param name="config">The adapter configuration.</param>
    /// <returns>A configured <see cref="ISmartHalAdapter"/> instance.</returns>
    ISmartHalAdapter CreateAdapter(AdapterConfig config);

    /// <summary>Returns the list of available adapter type identifiers.</summary>
    /// <returns>A read-only list of registered adapter type identifiers.</returns>
    IReadOnlyList<string> GetAvailableAdapterTypes();
}
