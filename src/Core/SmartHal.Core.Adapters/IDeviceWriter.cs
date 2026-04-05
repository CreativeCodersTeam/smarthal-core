using SmartHal.Core.Devices;

namespace SmartHal.Core.Adapters;

/// <summary>
/// Optional capability for adapters that can write device parameters.
/// </summary>
public interface IDeviceWriter
{
    /// <summary>Writes a single parameter value to a device.</summary>
    /// <param name="nativeId">The native device identifier.</param>
    /// <param name="parameterName">The name of the parameter to write.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="ct">The cancellation token.</param>
    Task WriteDeviceParameterAsync(string nativeId, string parameterName, ParameterValue value, CancellationToken ct = default);
}
