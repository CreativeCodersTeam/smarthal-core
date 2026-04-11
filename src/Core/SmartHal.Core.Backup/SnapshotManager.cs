using CreativeCoders.Core;
using Microsoft.Extensions.Logging;
using SmartHal.Core.Config;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SmartHal.Core.Backup;

/// <summary>
/// File-system based implementation of <see cref="ISnapshotManager"/>.
/// Each snapshot is stored as its own directory under the snapshots root and contains
/// a <c>manifest.yaml</c> plus the captured device YAML files (and optionally adapter backups).
/// </summary>
public class SnapshotManager : ISnapshotManager
{
    private readonly string _snapshotsRoot;
    private readonly IConfigRepository _configRepository;
    private readonly IAdapterLookup? _adapterLookup;
    private readonly ILogger<SnapshotManager> _logger;

    private readonly ISerializer _serializer = new SerializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .WithTypeConverter(new DateTimeOffsetYamlConverter())
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
        .Build();

    private readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .WithTypeConverter(new DateTimeOffsetYamlConverter())
        .IgnoreUnmatchedProperties()
        .Build();

    /// <summary>
    /// Initializes a new instance of the <see cref="SnapshotManager"/> class.
    /// </summary>
    /// <param name="snapshotsRoot">The root directory in which snapshots are stored.</param>
    /// <param name="configRepository">The configuration repository used to resolve scopes and read device data.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="adapterLookup">
    /// An optional adapter lookup. Required for <see cref="SnapshotMode.Embedded"/>;
    /// adapters that do not implement backup capability are silently skipped.
    /// </param>
    public SnapshotManager(string snapshotsRoot, IConfigRepository configRepository, ILogger<SnapshotManager> logger, IAdapterLookup? adapterLookup = null)
    {
        _snapshotsRoot = Ensure.IsNotNullOrWhitespace(snapshotsRoot);
        _configRepository = Ensure.NotNull(configRepository);
        _logger = Ensure.NotNull(logger);
        _adapterLookup = adapterLookup;
    }

    /// <inheritdoc />
    public async Task<SnapshotManifest> CreateSnapshotAsync(SnapshotRequest request, CancellationToken ct = default)
    {
        Ensure.NotNull(request);

        _logger.LogInformation("Creating snapshot for scope {Scope} (scopeId: {ScopeId})", request.Scope, request.ScopeId);

        // Resolve which devices belong to the requested scope.
        var devices = await ResolveDevicesAsync(request.Scope, request.ScopeId, ct).ConfigureAwait(false);

        _logger.LogDebug("Resolved {DeviceCount} device(s) for snapshot scope {Scope}", devices.Count, request.Scope);

        // Build the snapshot id and directory layout up front so we can fail fast on disk errors.
        var snapshotId = BuildSnapshotId(request.Scope, request.ScopeId);
        var snapshotDir = SnapshotPaths.GetSnapshotDirectory(_snapshotsRoot, snapshotId);
        Directory.CreateDirectory(snapshotDir);
        Directory.CreateDirectory(Path.Combine(snapshotDir, SnapshotPaths.DevicesDirectoryName));

        var manifest = new SnapshotManifest
        {
            SnapshotId = snapshotId,
            CreatedAt = DateTimeOffset.UtcNow,
            Scope = request.Scope,
            ScopeId = request.ScopeId,
            Mode = request.Mode,
            Description = request.Description,
            Trigger = request.Trigger
        };

        foreach (var summary in devices)
        {
            ct.ThrowIfCancellationRequested();

            _logger.LogDebug("Snapshotting device {DeviceId}", summary.Id);

            var device = await _configRepository.GetDeviceAsync(summary.Id, ct).ConfigureAwait(false);
            var configRelative = SnapshotPaths.GetDeviceConfigRelativePath(device.Id);
            var configAbsolute = Path.Combine(snapshotDir, configRelative);

            // Persist the device YAML directly via the same serializer to keep the snapshot self-contained.
            await WriteYamlAsync(configAbsolute, device, ct).ConfigureAwait(false);

            var entry = new ManifestEntry
            {
                DeviceId = device.Id,
                AdapterId = device.AdapterId,
                NativeId = device.NativeId,
                ConfigFilePath = configRelative
            };

            // Embedded mode: ask the adapter for its backup blob, when available.
            if (request.Mode == SnapshotMode.Embedded)
            {
                var capability = _adapterLookup?.GetBackupCapability(device.AdapterId);
                if (capability is not null)
                {
                    var blob = await capability.BackupDeviceAsync(device.NativeId, ct).ConfigureAwait(false);
                    var backupRelative = SnapshotPaths.GetAdapterBackupRelativePath(device.Id);
                    var backupAbsolute = Path.Combine(snapshotDir, backupRelative);
                    await WriteYamlAsync(backupAbsolute, blob, ct).ConfigureAwait(false);
                    entry.AdapterBackupPath = backupRelative;

                    _logger.LogDebug("Captured adapter backup for device {DeviceId}", device.Id);
                }
            }

            manifest.Entries.Add(entry);
        }

        // Manifest is written last so a partially-built snapshot is recognizable as incomplete.
        var manifestPath = SnapshotPaths.GetManifestPath(_snapshotsRoot, snapshotId);
        await WriteYamlAsync(manifestPath, manifest, ct).ConfigureAwait(false);

        _logger.LogInformation("Snapshot {SnapshotId} created with {DeviceCount} device(s)", snapshotId, manifest.Entries.Count);

        return manifest;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SnapshotManifest>> ListSnapshotsAsync(SnapshotFilter? filter = null, CancellationToken ct = default)
    {
        _logger.LogDebug("Listing snapshots from {SnapshotsRoot}", _snapshotsRoot);

        if (!Directory.Exists(_snapshotsRoot))
        {
            _logger.LogWarning("Snapshots root directory does not exist at {SnapshotsRoot}", _snapshotsRoot);
            return [];
        }

        var results = new List<SnapshotManifest>();

        foreach (var dir in Directory.GetDirectories(_snapshotsRoot))
        {
            ct.ThrowIfCancellationRequested();

            var manifestPath = Path.Combine(dir, SnapshotPaths.ManifestFileName);
            if (!File.Exists(manifestPath))
            {
                _logger.LogDebug("Skipping directory without manifest: {Directory}", dir);
                // Skip incomplete snapshots without a manifest.
                continue;
            }

            var manifest = await ReadManifestAsync(manifestPath, ct).ConfigureAwait(false);

            if (MatchesFilter(manifest, filter))
            {
                results.Add(manifest);
            }
        }

        _logger.LogDebug("Found {Count} snapshot(s) matching filter", results.Count);

        // Newest first — most call sites care about recent snapshots.
        return results
            .OrderByDescending(m => m.CreatedAt)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<SnapshotManifest> GetSnapshotAsync(string snapshotId, CancellationToken ct = default)
    {
        Ensure.IsNotNullOrWhitespace(snapshotId);

        _logger.LogDebug("Reading snapshot {SnapshotId}", snapshotId);

        var manifestPath = SnapshotPaths.GetManifestPath(_snapshotsRoot, snapshotId);
        if (!File.Exists(manifestPath))
        {
            throw new SmartHalException($"Snapshot '{snapshotId}' not found.");
        }

        return await ReadManifestAsync(manifestPath, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task DeleteSnapshotAsync(string snapshotId, CancellationToken ct = default)
    {
        Ensure.IsNotNullOrWhitespace(snapshotId);

        _logger.LogInformation("Deleting snapshot {SnapshotId}", snapshotId);

        var snapshotDir = SnapshotPaths.GetSnapshotDirectory(_snapshotsRoot, snapshotId);
        if (!Directory.Exists(snapshotDir))
        {
            throw new SmartHalException($"Snapshot '{snapshotId}' not found.");
        }

        Directory.Delete(snapshotDir, recursive: true);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<int> ApplyRetentionPolicyAsync(RetentionPolicy policy, CancellationToken ct = default)
    {
        Ensure.NotNull(policy);

        _logger.LogInformation("Applying retention policy (MaxAge: {MaxAge}, MaxSnapshots: {MaxSnapshots})", policy.MaxAge, policy.MaxSnapshots);

        var snapshots = await ListSnapshotsAsync(filter: null, ct).ConfigureAwait(false);

        // Determine the candidate set: manual snapshots are exempt when KeepManualSnapshots is true.
        var candidates = policy.KeepManualSnapshots
            ? snapshots.Where(s => !string.Equals(s.Trigger, "manual", StringComparison.OrdinalIgnoreCase)).ToList()
            : snapshots.ToList();

        var toDelete = new HashSet<string>(StringComparer.Ordinal);

        // Age based: delete anything older than MaxAge.
        if (policy.MaxAge.HasValue)
        {
            var cutoff = DateTimeOffset.UtcNow - policy.MaxAge.Value;
            foreach (var snapshot in candidates.Where(s => s.CreatedAt < cutoff))
            {
                toDelete.Add(snapshot.SnapshotId);
            }
        }

        // Count based: keep only the newest MaxSnapshots from the candidate set.
        if (policy.MaxSnapshots.HasValue && candidates.Count > policy.MaxSnapshots.Value)
        {
            var excess = candidates
                .OrderByDescending(s => s.CreatedAt)
                .Skip(policy.MaxSnapshots.Value);

            foreach (var snapshot in excess)
            {
                toDelete.Add(snapshot.SnapshotId);
            }
        }

        foreach (var snapshotId in toDelete)
        {
            ct.ThrowIfCancellationRequested();
            await DeleteSnapshotAsync(snapshotId, ct).ConfigureAwait(false);
        }

        _logger.LogInformation("Retention policy deleted {DeletedCount} snapshot(s)", toDelete.Count);

        return toDelete.Count;
    }

    // --- Helpers ---

    /// <summary>
    /// Resolves the configured scope into the concrete list of device summaries that should be snapshotted.
    /// </summary>
    private async Task<IReadOnlyList<DeviceSummary>> ResolveDevicesAsync(ConfigScope scope, string? scopeId, CancellationToken ct)
    {
        switch (scope)
        {
            case ConfigScope.Device:
                if (string.IsNullOrWhiteSpace(scopeId))
                {
                    throw new SmartHalException("ScopeId is required for ConfigScope.Device.");
                }
                // Validate the device exists; this throws when not.
                var device = await _configRepository.GetDeviceAsync(scopeId, ct).ConfigureAwait(false);
                var singleDeviceResult = new[]
                {
                    new DeviceSummary
                    {
                        Id = device.Id,
                        AdapterId = device.AdapterId,
                        NativeId = device.NativeId,
                        Name = device.Name,
                        RoomId = device.RoomId
                    }
                };
                _logger.LogDebug("Resolved scope {Scope} to {DeviceCount} device(s)", scope, singleDeviceResult.Length);
                return singleDeviceResult;

            case ConfigScope.Adapter:
                if (string.IsNullOrWhiteSpace(scopeId))
                {
                    throw new SmartHalException("ScopeId is required for ConfigScope.Adapter.");
                }
                var adapterDevices = await _configRepository
                    .ListDevicesAsync(new DeviceFilter { AdapterId = scopeId }, ct)
                    .ConfigureAwait(false);
                _logger.LogDebug("Resolved scope {Scope} to {DeviceCount} device(s)", scope, adapterDevices.Count);
                return adapterDevices;

            case ConfigScope.Room:
                if (string.IsNullOrWhiteSpace(scopeId))
                {
                    throw new SmartHalException("ScopeId is required for ConfigScope.Room.");
                }
                var roomDevices = await _configRepository
                    .ListDevicesAsync(new DeviceFilter { RoomId = scopeId }, ct)
                    .ConfigureAwait(false);
                _logger.LogDebug("Resolved scope {Scope} to {DeviceCount} device(s)", scope, roomDevices.Count);
                return roomDevices;

            case ConfigScope.All:
                var allDevices = await _configRepository.ListDevicesAsync(ct).ConfigureAwait(false);
                _logger.LogDebug("Resolved scope {Scope} to {DeviceCount} device(s)", scope, allDevices.Count);
                return allDevices;

            default:
                throw new SmartHalException($"Unsupported ConfigScope '{scope}'.");
        }
    }

    /// <summary>
    /// Builds a deterministic, file-system safe snapshot identifier of the form
    /// <c>{timestamp}_{scope}[_{scopeId}]</c>.
    /// </summary>
    internal static string BuildSnapshotId(ConfigScope scope, string? scopeId)
    {
        // Use a file-name safe ISO-like timestamp (ASCII, no colons).
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH-mm-ss");
        var scopeName = scope.ToString().ToLowerInvariant();

        return scope == ConfigScope.All
            ? $"{timestamp}_{scopeName}"
            : $"{timestamp}_{scopeName}_{scopeId}";
    }

    private static bool MatchesFilter(SnapshotManifest manifest, SnapshotFilter? filter)
    {
        if (filter is null)
        {
            return true;
        }

        if (filter.Scope.HasValue && manifest.Scope != filter.Scope.Value)
        {
            return false;
        }

        if (filter.ScopeId is not null && !string.Equals(manifest.ScopeId, filter.ScopeId, StringComparison.Ordinal))
        {
            return false;
        }

        if (filter.Trigger is not null && !string.Equals(manifest.Trigger, filter.Trigger, StringComparison.Ordinal))
        {
            return false;
        }

        if (filter.Since.HasValue && manifest.CreatedAt < filter.Since.Value)
        {
            return false;
        }

        if (filter.Until.HasValue && manifest.CreatedAt > filter.Until.Value)
        {
            return false;
        }

        return true;
    }

    private async Task<SnapshotManifest> ReadManifestAsync(string manifestPath, CancellationToken ct)
    {
        var yaml = await File.ReadAllTextAsync(manifestPath, ct).ConfigureAwait(false);
        return _deserializer.Deserialize<SnapshotManifest>(yaml) ?? new SnapshotManifest();
    }

    private async Task WriteYamlAsync<T>(string filePath, T data, CancellationToken ct)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (directory is not null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var yaml = _serializer.Serialize(data!);
        await File.WriteAllTextAsync(filePath, yaml, ct).ConfigureAwait(false);
    }
}
