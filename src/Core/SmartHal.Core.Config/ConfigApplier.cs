using SmartHal.Core.Adapters;
using SmartHal.Core.Devices;

namespace SmartHal.Core.Config;

/// <summary>
/// Applies device diffs to an adapter by writing changed parameters.
/// Collects errors per parameter and throws an aggregated exception if any fail.
/// </summary>
public class ConfigApplier : IConfigApplier
{
    /// <inheritdoc />
    public async Task ApplyDiffAsync(DeviceDiff diff, ISmartHalAdapter adapter, string nativeId, CancellationToken ct = default)
    {
        if (adapter is not IDeviceWriter writer)
        {
            throw new SmartHalAdapterOperationException(
                $"Adapter '{adapter.AdapterId}' does not support writing device parameters.",
                adapter.AdapterId);
        }

        var errors = new List<string>();

        foreach (var change in diff.Changes.Where(c => c.Kind is DiffKind.Changed or DiffKind.Added))
        {
            // Only apply parameter changes (path starts with "parameters.")
            if (!change.Path.StartsWith("parameters.", StringComparison.Ordinal))
            {
                continue;
            }

            var parameterName = change.Path["parameters.".Length..];

            try
            {
                var value = ParameterValue.FromString(change.NewValue ?? "");
                await writer.WriteDeviceParameterAsync(nativeId, parameterName, value, ct).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                errors.Add($"Failed to write parameter '{parameterName}': {ex.Message}");
            }
        }

        if (errors.Count > 0)
        {
            throw new SmartHalAdapterOperationException(
                $"Failed to apply {errors.Count} parameter(s): {string.Join("; ", errors)}",
                adapter.AdapterId);
        }
    }
}
