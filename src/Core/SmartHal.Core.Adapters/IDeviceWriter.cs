using SmartHal.Core.Devices;

namespace SmartHal.Core.Adapters;

/// <summary>
/// Optional capability for adapters that can write device parameters.
/// </summary>
public interface IDeviceWriter
{
    /// <summary>Writes a single parameter value to a device.</summary>
    Task WriteDeviceParameterAsync(string nativeId, string parameterName, ParameterValue value, CancellationToken ct = default);
}
