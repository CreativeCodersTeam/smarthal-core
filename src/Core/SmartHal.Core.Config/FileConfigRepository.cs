using System.Text.RegularExpressions;
using CreativeCoders.Core;
using Microsoft.Extensions.Logging;
using SmartHal.Core.Adapters;
using SmartHal.Core.Devices;

namespace SmartHal.Core.Config;

/// <summary>
/// File-system-based implementation of <see cref="IConfigRepository"/>.
/// Devices are stored as individual YAML files in the <c>devices/</c> subdirectory.
/// Adapter configurations are stored in the <c>adapters/</c> subdirectory.
/// </summary>
public class FileConfigRepository : IConfigRepository
{
    private readonly string _configPath;
    private readonly IConfigReader _reader;
    private readonly IConfigWriter _writer;
    private readonly ILogger<FileConfigRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileConfigRepository"/> class.
    /// </summary>
    /// <param name="configPath">The root configuration directory path.</param>
    /// <param name="reader">The configuration reader.</param>
    /// <param name="writer">The configuration writer.</param>
    /// <param name="logger">The logger instance.</param>
    public FileConfigRepository(string configPath, IConfigReader reader, IConfigWriter writer, ILogger<FileConfigRepository> logger)
    {
        _configPath = Ensure.IsNotNullOrWhitespace(configPath);
        _reader = Ensure.NotNull(reader);
        _writer = Ensure.NotNull(writer);
        _logger = Ensure.NotNull(logger);
    }

    // --- Meta ---

    /// <inheritdoc />
    public async Task<MetaConfig> GetMetaAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Reading meta config from {ConfigPath}", _configPath);

        return await _reader.ReadMetaAsync(_configPath, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task SaveMetaAsync(MetaConfig meta, CancellationToken ct = default)
    {
        Ensure.NotNull(meta);

        await _writer.WriteMetaAsync(_configPath, meta, ct).ConfigureAwait(false);

        _logger.LogInformation("Saved meta config");
    }

    /// <inheritdoc />
    public async Task InitializeAsync(MetaConfig meta, CancellationToken ct = default)
    {
        Ensure.NotNull(meta);

        await _writer.InitializeDirectoryStructureAsync(_configPath, ct).ConfigureAwait(false);
        await _writer.WriteMetaAsync(_configPath, meta, ct).ConfigureAwait(false);

        _logger.LogInformation("Initialized config directory at {ConfigPath}", _configPath);
    }

    // --- Rooms & Groups ---

    /// <inheritdoc />
    public async Task<RoomsConfig> GetRoomsAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Reading rooms config");

        return await _reader.ReadRoomsAsync(_configPath, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task SaveRoomsAsync(RoomsConfig rooms, CancellationToken ct = default)
    {
        Ensure.NotNull(rooms);

        await _writer.WriteRoomsAsync(_configPath, rooms, ct).ConfigureAwait(false);

        _logger.LogInformation("Saved rooms config");
    }

    /// <inheritdoc />
    public async Task<AdapterConfig> GetAdapterConfigAsync(string adapterId, CancellationToken ct = default)
    {
        Ensure.IsNotNullOrWhitespace(adapterId);

        _logger.LogDebug("Reading adapter config for {AdapterId}", adapterId);

        var filePath = GetAdapterFilePath(adapterId);

        return await _reader.ReadAdapterConfigAsync(filePath, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AdapterConfig>> GetAllAdapterConfigsAsync(CancellationToken ct = default)
    {
        var adaptersDir = Path.Combine(_configPath, "adapters");

        _logger.LogDebug("Loading all adapter configs from {AdaptersDir}", adaptersDir);

        if (!Directory.Exists(adaptersDir))
        {
            _logger.LogWarning("Adapters directory does not exist at {AdaptersDir}", adaptersDir);
            return [];
        }

        var configs = new List<AdapterConfig>();

        foreach (var file in Directory.GetFiles(adaptersDir, "*.yaml"))
        {
            var config = await _reader.ReadAdapterConfigAsync(file, ct).ConfigureAwait(false);
            configs.Add(config);
        }

        return configs;
    }

    /// <inheritdoc />
    public async Task SaveAdapterConfigAsync(AdapterConfig config, CancellationToken ct = default)
    {
        Ensure.NotNull(config);

        var filePath = GetAdapterFilePath(config.AdapterId);

        await _writer.WriteAdapterConfigAsync(filePath, config, ct).ConfigureAwait(false);

        _logger.LogInformation("Saved adapter config for {AdapterId}", config.AdapterId);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DeviceSummary>> ListDevicesAsync(CancellationToken ct = default)
    {
        var devicesDir = Path.Combine(_configPath, "devices");

        _logger.LogDebug("Listing devices from {DevicesDir}", devicesDir);

        if (!Directory.Exists(devicesDir))
        {
            _logger.LogWarning("Devices directory does not exist at {DevicesDir}", devicesDir);
            return [];
        }

        var summaries = new List<DeviceSummary>();

        foreach (var file in Directory.GetFiles(devicesDir, "*.yaml"))
        {
            var summary = await _reader.ReadDeviceSummaryAsync(file, ct).ConfigureAwait(false);
            summaries.Add(summary);
        }

        return summaries;
    }

    /// <inheritdoc />
    public async Task<Device> GetDeviceAsync(string deviceId, CancellationToken ct = default)
    {
        Ensure.IsNotNullOrWhitespace(deviceId);

        _logger.LogDebug("Reading device {DeviceId}", deviceId);

        var filePath = GetDeviceFilePath(deviceId);

        return await _reader.ReadDeviceAsync(filePath, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task SaveDeviceAsync(Device device, CancellationToken ct = default)
    {
        Ensure.NotNull(device);

        var filePath = GetDeviceFilePath(device.Id);

        await _writer.WriteDeviceAsync(filePath, device, ct).ConfigureAwait(false);

        _logger.LogInformation("Saved device {DeviceId}", device.Id);
    }

    /// <inheritdoc />
    public async Task DeleteDeviceAsync(string deviceId, CancellationToken ct = default)
    {
        Ensure.IsNotNullOrWhitespace(deviceId);

        var filePath = GetDeviceFilePath(deviceId);

        await _writer.DeleteDeviceFileAsync(filePath, ct).ConfigureAwait(false);

        _logger.LogInformation("Deleted device {DeviceId}", deviceId);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DeviceSummary>> ListDevicesAsync(DeviceFilter filter, CancellationToken ct = default)
    {
        Ensure.NotNull(filter);

        _logger.LogDebug("Listing devices with filter");

        var allDevices = await ListDevicesAsync(ct).ConfigureAwait(false);

        IEnumerable<DeviceSummary> filtered = allDevices;

        if (filter.AdapterId is not null)
        {
            filtered = filtered.Where(d => d.AdapterId == filter.AdapterId);
        }

        if (filter.RoomId is not null)
        {
            filtered = filtered.Where(d => d.RoomId == filter.RoomId);
        }

        if (filter.Type is not null)
        {
            filtered = filtered.Where(d => d.Type == filter.Type.Value);
        }

        if (filter.NamePattern is not null)
        {
            var regex = GlobToRegex(filter.NamePattern);
            filtered = filtered.Where(d => regex.IsMatch(d.Name));
        }

        // GroupId filter requires full device loading
        if (filter.GroupId is not null)
        {
            var matchingIds = new List<string>();

            foreach (var summary in filtered)
            {
                var device = await _reader.ReadDeviceAsync(
                    GetDeviceFilePath(summary.Id), ct).ConfigureAwait(false);

                if (device.GroupIds.Contains(filter.GroupId))
                {
                    matchingIds.Add(summary.Id);
                }
            }

            filtered = allDevices.Where(d => matchingIds.Contains(d.Id));
        }

        return filtered.ToList();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // No unmanaged resources to dispose.
        GC.SuppressFinalize(this);
    }

    private string GetDeviceFilePath(string deviceId) =>
        Path.Combine(_configPath, "devices", $"{deviceId}.yaml");

    private string GetAdapterFilePath(string adapterId) =>
        Path.Combine(_configPath, "adapters", $"{adapterId}.yaml");

    /// <summary>
    /// Converts a simple glob pattern (supporting * and ?) to a regex.
    /// </summary>
    /// <param name="pattern">The glob pattern to convert.</param>
    /// <returns>A compiled <see cref="Regex"/> equivalent of the glob pattern.</returns>
    private static Regex GlobToRegex(string pattern)
    {
        var escaped = Regex.Escape(pattern)
            .Replace("\\*", ".*", StringComparison.Ordinal)
            .Replace("\\?", ".", StringComparison.Ordinal);

        return new Regex($"^{escaped}$", RegexOptions.IgnoreCase);
    }
}
