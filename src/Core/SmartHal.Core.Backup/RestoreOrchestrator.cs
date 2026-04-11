using CreativeCoders.Core;
using Microsoft.Extensions.Logging;
using SmartHal.Core.Config;
using SmartHal.Core.Devices;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SmartHal.Core.Backup;

/// <summary>
/// Default implementation of <see cref="IRestoreOrchestrator"/>.
/// Handles preview, automatic pre-restore snapshot creation and per-device error tolerance.
/// </summary>
public class RestoreOrchestrator : IRestoreOrchestrator
{
    private readonly string _snapshotsRoot;
    private readonly ISnapshotManager _snapshotManager;
    private readonly IConfigRepository _configRepository;
    private readonly IConfigReader _configReader;
    private readonly IConfigDiffer _differ;
    private readonly IAdapterLookup? _adapterLookup;
    private readonly ILogger<RestoreOrchestrator> _logger;

    private readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .WithTypeConverter(new DateTimeOffsetYamlConverter())
        .IgnoreUnmatchedProperties()
        .Build();

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreOrchestrator"/> class.
    /// </summary>
    /// <param name="snapshotsRoot">The root directory in which snapshots are stored.</param>
    /// <param name="snapshotManager">The snapshot manager used to read manifests and create pre-restore snapshots.</param>
    /// <param name="configRepository">The live configuration repository.</param>
    /// <param name="configReader">The reader used to parse snapshot device YAML files.</param>
    /// <param name="differ">The diff computer.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="adapterLookup">Optional adapter lookup used to apply embedded adapter backups.</param>
    public RestoreOrchestrator(
        string snapshotsRoot,
        ISnapshotManager snapshotManager,
        IConfigRepository configRepository,
        IConfigReader configReader,
        IConfigDiffer differ,
        ILogger<RestoreOrchestrator> logger,
        IAdapterLookup? adapterLookup = null)
    {
        _snapshotsRoot = Ensure.IsNotNullOrWhitespace(snapshotsRoot);
        _snapshotManager = Ensure.NotNull(snapshotManager);
        _configRepository = Ensure.NotNull(configRepository);
        _configReader = Ensure.NotNull(configReader);
        _differ = Ensure.NotNull(differ);
        _logger = Ensure.NotNull(logger);
        _adapterLookup = adapterLookup;
    }

    /// <inheritdoc />
    public async Task<RestorePreviewResult> PreviewRestoreAsync(string snapshotId, CancellationToken ct = default)
    {
        Ensure.IsNotNullOrWhitespace(snapshotId);

        _logger.LogInformation("Previewing restore for snapshot {SnapshotId}", snapshotId);

        var manifest = await _snapshotManager.GetSnapshotAsync(snapshotId, ct).ConfigureAwait(false);
        var liveDeviceIds = await GetLiveDeviceIdsAsync(ct).ConfigureAwait(false);

        var preview = new RestorePreviewResult { SnapshotId = snapshotId };

        foreach (var entry in manifest.Entries)
        {
            ct.ThrowIfCancellationRequested();

            var snapshotDevice = await ReadSnapshotDeviceAsync(snapshotId, entry, ct).ConfigureAwait(false);
            var deviceExists = liveDeviceIds.Contains(entry.DeviceId);

            DeviceDiff diff;
            if (deviceExists)
            {
                var current = await _configRepository.GetDeviceAsync(entry.DeviceId, ct).ConfigureAwait(false);
                diff = _differ.ComputeDiff(current, snapshotDevice);
            }
            else
            {
                // Device was deleted; restoring it counts as a full re-add. Diff against an empty baseline.
                diff = _differ.ComputeDiff(new Device { Id = entry.DeviceId }, snapshotDevice);
            }

            preview.Devices.Add(new DeviceRestorePreview
            {
                DeviceId = entry.DeviceId,
                Diff = diff,
                DeviceExists = deviceExists
            });

            _logger.LogDebug("Preview: device {DeviceId} — exists: {DeviceExists}, changes: {ChangeCount}", entry.DeviceId, deviceExists, diff.Changes.Count);
        }

        _logger.LogInformation("Preview complete: {DeviceCount} device(s) affected", preview.Devices.Count);

        return preview;
    }

    /// <inheritdoc />
    public async Task<RestoreResult> RestoreAsync(string snapshotId, bool force = false, CancellationToken ct = default)
    {
        Ensure.IsNotNullOrWhitespace(snapshotId);

        _logger.LogInformation("Restoring snapshot {SnapshotId} (force: {Force})", snapshotId, force);

        var manifest = await _snapshotManager.GetSnapshotAsync(snapshotId, ct).ConfigureAwait(false);

        // Step 1: Always create a pre-restore safety snapshot of the current state.
        // We snapshot the same scope as the snapshot we are about to restore so the safety net
        // matches the affected device set as closely as possible.
        await _snapshotManager.CreateSnapshotAsync(
            new SnapshotRequest
            {
                Scope = manifest.Scope,
                ScopeId = manifest.ScopeId,
                Mode = SnapshotMode.Reference,
                Description = $"Auto snapshot before restoring '{snapshotId}'.",
                Trigger = "pre-restore"
            },
            ct).ConfigureAwait(false);

        _logger.LogInformation("Created pre-restore safety snapshot");

        // Step 2: Optionally bail out early if there are no changes and force was not requested.
        if (!force)
        {
            var preview = await PreviewRestoreAsync(snapshotId, ct).ConfigureAwait(false);
            if (!preview.HasChanges)
            {
                _logger.LogDebug("No changes detected, skipping restore");
                return new RestoreResult { SnapshotId = snapshotId };
            }
        }

        // Step 3: Restore each device individually. Errors are collected so a single failure
        // does not abort the entire restore operation.
        var result = new RestoreResult { SnapshotId = snapshotId };

        foreach (var entry in manifest.Entries)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                _logger.LogDebug("Restoring device {DeviceId}", entry.DeviceId);

                var snapshotDevice = await ReadSnapshotDeviceAsync(snapshotId, entry, ct).ConfigureAwait(false);
                await _configRepository.SaveDeviceAsync(snapshotDevice, ct).ConfigureAwait(false);

                // Apply embedded adapter backup data when present.
                if (entry.AdapterBackupPath is not null && _adapterLookup is not null)
                {
                    _logger.LogDebug("Applying adapter backup for device {DeviceId}", entry.DeviceId);

                    var capability = _adapterLookup.GetBackupCapability(entry.AdapterId);
                    if (capability is not null)
                    {
                        var blob = await ReadBackupBlobAsync(snapshotId, entry.AdapterBackupPath, ct).ConfigureAwait(false);
                        await capability.RestoreDeviceAsync(entry.NativeId, blob, ct).ConfigureAwait(false);
                    }
                }

                result.DevicesRestored++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Failed to restore device {DeviceId}: {ErrorMessage}", entry.DeviceId, ex.Message);

                result.Errors.Add(new RestoreError
                {
                    DeviceId = entry.DeviceId,
                    Message = ex.Message
                });
            }
        }

        _logger.LogInformation("Restore completed: {RestoredCount} device(s) restored, {ErrorCount} error(s)", result.DevicesRestored, result.Errors.Count);

        return result;
    }

    // --- Helpers ---

    private async Task<HashSet<string>> GetLiveDeviceIdsAsync(CancellationToken ct)
    {
        var summaries = await _configRepository.ListDevicesAsync(ct).ConfigureAwait(false);
        return new HashSet<string>(summaries.Select(s => s.Id), StringComparer.Ordinal);
    }

    private async Task<Device> ReadSnapshotDeviceAsync(string snapshotId, ManifestEntry entry, CancellationToken ct)
    {
        var path = Path.Combine(SnapshotPaths.GetSnapshotDirectory(_snapshotsRoot, snapshotId), entry.ConfigFilePath);
        return await _configReader.ReadDeviceAsync(path, ct).ConfigureAwait(false);
    }

    private async Task<BackupBlob> ReadBackupBlobAsync(string snapshotId, string backupRelativePath, CancellationToken ct)
    {
        var path = Path.Combine(SnapshotPaths.GetSnapshotDirectory(_snapshotsRoot, snapshotId), backupRelativePath);
        var yaml = await File.ReadAllTextAsync(path, ct).ConfigureAwait(false);
        return _deserializer.Deserialize<BackupBlob>(yaml) ?? new BackupBlob();
    }
}
