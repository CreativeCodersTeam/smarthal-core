using CreativeCoders.Core;
using Microsoft.Extensions.Logging;
using SmartHal.Core.Adapters;
using SmartHal.Core.Devices;

namespace SmartHal.Core.Config;

/// <summary>
/// Applies device diffs to an adapter by writing changed parameters.
/// Collects errors per parameter and throws an aggregated exception if any fail.
/// </summary>
public class ConfigApplier(ILogger<ConfigApplier> logger) : IConfigApplier
{
    private readonly ILogger<ConfigApplier> _logger = Ensure.NotNull(logger);

    /// <inheritdoc />
    public async Task ApplyDiffAsync(DeviceDiff diff, ISmartHalAdapter adapter, string nativeId, CancellationToken ct = default)
    {
        _logger.LogInformation("Applying diff for device {DeviceId} via adapter {AdapterId}", diff.DeviceId, adapter.AdapterId);

        if (adapter is not IDeviceWriter writer)
        {
            _logger.LogWarning("Adapter {AdapterId} does not support writing", adapter.AdapterId);

            throw new SmartHalAdapterOperationException(
                $"Adapter '{adapter.AdapterId}' does not support writing device parameters.",
                adapter.AdapterId);
        }

        var errors = new List<string>();
        var changeCount = 0;

        foreach (var change in diff.Changes.Where(c => c.Kind is DiffKind.Changed or DiffKind.Added))
        {
            // Only apply parameter changes (path starts with "parameters.")
            if (!change.Path.StartsWith("parameters.", StringComparison.Ordinal))
            {
                continue;
            }

            var parameterName = change.Path["parameters.".Length..];
            changeCount++;

            try
            {
                _logger.LogDebug("Writing parameter {ParameterName} for device {NativeId}", parameterName, nativeId);

                var value = ParameterValue.FromString(change.NewValue ?? "");
                await writer.WriteDeviceParameterAsync(nativeId, parameterName, value, ct).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Failed to write parameter {ParameterName}", parameterName);

                errors.Add($"Failed to write parameter '{parameterName}': {ex.Message}");
            }
        }

        if (errors.Count > 0)
        {
            throw new SmartHalAdapterOperationException(
                $"Failed to apply {errors.Count} parameter(s): {string.Join("; ", errors)}",
                adapter.AdapterId);
        }

        _logger.LogInformation("Successfully applied {ChangeCount} parameter change(s) for device {DeviceId}", changeCount, diff.DeviceId);
    }
}
