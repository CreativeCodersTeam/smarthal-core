namespace SmartHal.Core.Adapters;

/// <summary>
/// Factory for creating adapter instances by their configured type.
/// </summary>
public interface IAdapterFactory
{
    /// <summary>Creates an adapter instance from the given configuration.</summary>
    ISmartHalAdapter CreateAdapter(AdapterConfig config);

    /// <summary>Returns the list of available adapter type identifiers.</summary>
    IReadOnlyList<string> GetAvailableAdapterTypes();
}
