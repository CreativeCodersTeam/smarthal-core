using CreativeCoders.Core;
using Microsoft.Extensions.Logging;

namespace SmartHal.Core.Config;

/// <summary>
/// Validates configuration structure and semantic correctness.
/// </summary>
public class ConfigValidator : IConfigValidator
{
    private readonly IConfigReader _reader;
    private readonly ILogger<ConfigValidator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigValidator"/> class.
    /// </summary>
    /// <param name="reader">The configuration reader used for parsing YAML files during validation.</param>
    /// <param name="logger">The logger instance.</param>
    public ConfigValidator(IConfigReader reader, ILogger<ConfigValidator> logger)
    {
        _reader = reader;
        _logger = Ensure.NotNull(logger);
    }

    /// <inheritdoc />
    public async Task<ValidationResult> ValidateStructureAsync(string configPath, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting structure validation for {ConfigPath}", configPath);

        var result = new ValidationResult();

        // Check meta.yaml exists and is valid
        var metaPath = Path.Combine(configPath, "meta.yaml");
        if (!File.Exists(metaPath))
        {
            _logger.LogWarning("meta.yaml not found at {MetaPath}", metaPath);

            result.Errors.Add(new ValidationError
            {
                Code = "MISSING_META",
                Message = "meta.yaml not found.",
                FilePath = metaPath
            });
        }
        else
        {
            try
            {
                var meta = await _reader.ReadMetaAsync(configPath, ct).ConfigureAwait(false);
                if (meta.SchemaVersion != "1.0")
                {
                    _logger.LogWarning("Unsupported schema version {Version}", meta.SchemaVersion);

                    result.Errors.Add(new ValidationError
                    {
                        Code = "UNSUPPORTED_SCHEMA",
                        Message = $"Unsupported schema version: '{meta.SchemaVersion}'. Only '1.0' is supported.",
                        FilePath = metaPath
                    });
                }
            }
            catch (SmartHalConfigFileException ex)
            {
                _logger.LogError("Invalid YAML in {FilePath}: {Message}", ex.FilePath, ex.Message);

                result.Errors.Add(new ValidationError
                {
                    Code = "INVALID_YAML",
                    Message = ex.Message,
                    FilePath = ex.FilePath,
                    LineNumber = ex.LineNumber
                });
            }
        }

        // Validate adapter files
        var adaptersDir = Path.Combine(configPath, "adapters");
        if (Directory.Exists(adaptersDir))
        {
            await ValidateYamlFilesAsync(adaptersDir, result, ct).ConfigureAwait(false);
        }

        // Validate device files
        var devicesDir = Path.Combine(configPath, "devices");
        if (Directory.Exists(devicesDir))
        {
            await ValidateDeviceFilesAsync(devicesDir, result, ct).ConfigureAwait(false);
        }

        _logger.LogInformation("Structure validation completed with {ErrorCount} error(s)", result.Errors.Count);

        return result;
    }

    /// <inheritdoc />
    public async Task<ValidationResult> ValidateSemanticAsync(IConfigRepository repo, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting semantic validation");

        var result = new ValidationResult();

        var adapters = await repo.GetAllAdapterConfigsAsync(ct).ConfigureAwait(false);
        var rooms = await repo.GetRoomsAsync(ct).ConfigureAwait(false);
        var devices = await repo.ListDevicesAsync(ct).ConfigureAwait(false);

        _logger.LogDebug("Validating {DeviceCount} devices against {AdapterCount} adapters", devices.Count, adapters.Count);

        var adapterIds = adapters.Select(a => a.AdapterId).ToHashSet();
        var roomIds = rooms.Rooms.Select(r => r.Id).ToHashSet();
        var groupIds = rooms.Groups.Select(g => g.Id).ToHashSet();
        var deviceIds = new HashSet<string>();
        var nativeIdsByAdapter = new Dictionary<string, HashSet<string>>();

        foreach (var device in devices)
        {
            // Check for duplicate device IDs
            if (!deviceIds.Add(device.Id))
            {
                _logger.LogWarning("Duplicate device ID {DeviceId}", device.Id);

                result.Errors.Add(new ValidationError
                {
                    Code = "DUPLICATE_DEVICE_ID",
                    Message = $"Duplicate device ID: '{device.Id}'.",
                    FilePath = device.FilePath
                });
            }

            // Check adapter reference
            if (!adapterIds.Contains(device.AdapterId))
            {
                _logger.LogWarning("Device {DeviceId} references unknown adapter {AdapterId}", device.Id, device.AdapterId);

                result.Errors.Add(new ValidationError
                {
                    Code = "INVALID_ADAPTER_REF",
                    Message = $"Device '{device.Id}' references unknown adapter '{device.AdapterId}'.",
                    FilePath = device.FilePath
                });
            }

            // Check room reference
            if (device.RoomId is not null && !roomIds.Contains(device.RoomId))
            {
                _logger.LogWarning("Device {DeviceId} references unknown room {RoomId}", device.Id, device.RoomId);

                result.Errors.Add(new ValidationError
                {
                    Code = "INVALID_ROOM_REF",
                    Message = $"Device '{device.Id}' references unknown room '{device.RoomId}'.",
                    FilePath = device.FilePath
                });
            }

            // Check for duplicate native IDs within same adapter
            if (!nativeIdsByAdapter.TryGetValue(device.AdapterId, out var nativeIds))
            {
                nativeIds = [];
                nativeIdsByAdapter[device.AdapterId] = nativeIds;
            }

            if (!nativeIds.Add(device.NativeId))
            {
                _logger.LogWarning("Duplicate native ID {NativeId} in adapter {AdapterId}", device.NativeId, device.AdapterId);

                result.Errors.Add(new ValidationError
                {
                    Code = "DUPLICATE_NATIVE_ID",
                    Message = $"Duplicate native ID '{device.NativeId}' within adapter '{device.AdapterId}'.",
                    FilePath = device.FilePath
                });
            }
        }

        // Validate group references and relations require full device loading
        foreach (var device in devices)
        {
            var fullDevice = await repo.GetDeviceAsync(device.Id, ct).ConfigureAwait(false);

            foreach (var groupId in fullDevice.GroupIds.Where(gid => !groupIds.Contains(gid)))
            {
                _logger.LogWarning("Device {DeviceId} references unknown group {GroupId}", device.Id, groupId);

                result.Errors.Add(new ValidationError
                {
                    Code = "INVALID_GROUP_REF",
                    Message = $"Device '{device.Id}' references unknown group '{groupId}'.",
                    FilePath = device.FilePath
                });
            }

            foreach (var relation in fullDevice.Relations.Where(r => !deviceIds.Contains(r.TargetId)))
            {
                _logger.LogWarning("Device {DeviceId} has relation targeting unknown device {TargetId}", device.Id, relation.TargetId);

                result.Errors.Add(new ValidationError
                {
                    Code = "INVALID_RELATION_TARGET",
                    Message = $"Device '{device.Id}' has relation targeting unknown device '{relation.TargetId}'.",
                    FilePath = device.FilePath
                });
            }
        }

        _logger.LogInformation("Semantic validation completed with {ErrorCount} error(s)", result.Errors.Count);

        return result;
    }

    private async Task ValidateYamlFilesAsync(string directory, ValidationResult result, CancellationToken ct)
    {
        foreach (var file in Directory.GetFiles(directory, "*.yaml"))
        {
            try
            {
                await _reader.ReadAdapterConfigAsync(file, ct).ConfigureAwait(false);
            }
            catch (SmartHalConfigFileException ex)
            {
                _logger.LogError("Invalid YAML in {FilePath}: {Message}", ex.FilePath, ex.Message);

                result.Errors.Add(new ValidationError
                {
                    Code = "INVALID_YAML",
                    Message = ex.Message,
                    FilePath = ex.FilePath,
                    LineNumber = ex.LineNumber
                });
            }
        }
    }

    private async Task ValidateDeviceFilesAsync(string directory, ValidationResult result, CancellationToken ct)
    {
        foreach (var file in Directory.GetFiles(directory, "*.yaml"))
        {
            try
            {
                var summary = await _reader.ReadDeviceSummaryAsync(file, ct).ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(summary.Id))
                {
                    result.Errors.Add(new ValidationError { Code = "MISSING_FIELD", Message = "Device is missing 'id'.", FilePath = file });
                }

                if (string.IsNullOrWhiteSpace(summary.AdapterId))
                {
                    result.Errors.Add(new ValidationError { Code = "MISSING_FIELD", Message = "Device is missing 'adapter_id'.", FilePath = file });
                }

                if (string.IsNullOrWhiteSpace(summary.NativeId))
                {
                    result.Errors.Add(new ValidationError { Code = "MISSING_FIELD", Message = "Device is missing 'native_id'.", FilePath = file });
                }

                if (string.IsNullOrWhiteSpace(summary.Name))
                {
                    result.Errors.Add(new ValidationError { Code = "MISSING_FIELD", Message = "Device is missing 'name'.", FilePath = file });
                }
            }
            catch (SmartHalConfigFileException ex)
            {
                _logger.LogError("Invalid YAML in {FilePath}: {Message}", ex.FilePath, ex.Message);

                result.Errors.Add(new ValidationError
                {
                    Code = "INVALID_YAML",
                    Message = ex.Message,
                    FilePath = ex.FilePath,
                    LineNumber = ex.LineNumber
                });
            }
        }
    }
}
