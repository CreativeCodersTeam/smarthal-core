namespace SmartHal.Core.Config;

/// <summary>
/// Validates configuration structure and semantic correctness.
/// </summary>
public class ConfigValidator : IConfigValidator
{
    private readonly IConfigReader _reader;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigValidator"/> class.
    /// </summary>
    /// <param name="reader">The configuration reader used for parsing YAML files during validation.</param>
    public ConfigValidator(IConfigReader reader)
    {
        _reader = reader;
    }

    /// <inheritdoc />
    public async Task<ValidationResult> ValidateStructureAsync(string configPath, CancellationToken ct = default)
    {
        var result = new ValidationResult();

        // Check meta.yaml exists and is valid
        var metaPath = Path.Combine(configPath, "meta.yaml");
        if (!File.Exists(metaPath))
        {
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

        return result;
    }

    /// <inheritdoc />
    public async Task<ValidationResult> ValidateSemanticAsync(IConfigRepository repo, CancellationToken ct = default)
    {
        var result = new ValidationResult();

        var adapters = await repo.GetAllAdapterConfigsAsync(ct).ConfigureAwait(false);
        var rooms = await repo.GetRoomsAsync(ct).ConfigureAwait(false);
        var devices = await repo.ListDevicesAsync(ct).ConfigureAwait(false);

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
                result.Errors.Add(new ValidationError
                {
                    Code = "INVALID_GROUP_REF",
                    Message = $"Device '{device.Id}' references unknown group '{groupId}'.",
                    FilePath = device.FilePath
                });
            }

            foreach (var relation in fullDevice.Relations.Where(r => !deviceIds.Contains(r.TargetId)))
            {
                result.Errors.Add(new ValidationError
                {
                    Code = "INVALID_RELATION_TARGET",
                    Message = $"Device '{device.Id}' has relation targeting unknown device '{relation.TargetId}'.",
                    FilePath = device.FilePath
                });
            }
        }

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
