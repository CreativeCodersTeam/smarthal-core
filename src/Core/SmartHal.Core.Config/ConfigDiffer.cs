using SmartHal.Core.Devices;

namespace SmartHal.Core.Config;

/// <summary>
/// Compares two device states and produces a diff of changes.
/// </summary>
public class ConfigDiffer : IConfigDiffer
{
    /// <inheritdoc />
    public DeviceDiff ComputeDiff(Device baseline, Device current)
    {
        var diff = new DeviceDiff { DeviceId = current.Id };

        // Compare simple properties
        CompareProperty(diff, "name", baseline.Name, current.Name);
        CompareProperty(diff, "type", baseline.Type.Value, current.Type.Value);
        CompareProperty(diff, "room_id", baseline.RoomId ?? "", current.RoomId ?? "");

        // Compare group IDs
        CompareListProperty(diff, "group_ids", baseline.GroupIds, current.GroupIds);

        // Compare device-level parameters
        CompareParameters(diff, "parameters", baseline.Parameters, current.Parameters);

        // Compare channels
        CompareChannels(diff, baseline.Channels, current.Channels);

        // Compare relations
        CompareRelations(diff, baseline.Relations, current.Relations);

        return diff;
    }

    private static void CompareProperty(DeviceDiff diff, string path, string oldValue, string newValue)
    {
        if (oldValue != newValue)
        {
            diff.Changes.Add(new DiffEntry
            {
                Kind = DiffKind.Changed,
                Path = path,
                OldValue = oldValue,
                NewValue = newValue
            });
        }
    }

    private static void CompareListProperty(DeviceDiff diff, string path, List<string> baseline, List<string> current)
    {
        var added = current.Except(baseline).ToList();
        var removed = baseline.Except(current).ToList();

        foreach (var item in added)
        {
            diff.Changes.Add(new DiffEntry { Kind = DiffKind.Added, Path = $"{path}.{item}", NewValue = item });
        }

        foreach (var item in removed)
        {
            diff.Changes.Add(new DiffEntry { Kind = DiffKind.Removed, Path = $"{path}.{item}", OldValue = item });
        }
    }

    private static void CompareParameters(DeviceDiff diff, string basePath,
        Dictionary<string, ParameterValue> baseline, Dictionary<string, ParameterValue> current)
    {
        foreach (var (key, value) in current)
        {
            if (baseline.TryGetValue(key, out var oldValue))
            {
                if (oldValue != value)
                {
                    diff.Changes.Add(new DiffEntry
                    {
                        Kind = DiffKind.Changed,
                        Path = $"{basePath}.{key}",
                        OldValue = FormatParameterValue(oldValue),
                        NewValue = FormatParameterValue(value)
                    });
                }
            }
            else
            {
                diff.Changes.Add(new DiffEntry
                {
                    Kind = DiffKind.Added,
                    Path = $"{basePath}.{key}",
                    NewValue = FormatParameterValue(value)
                });
            }
        }

        foreach (var key in baseline.Keys.Where(k => !current.ContainsKey(k)))
        {
            diff.Changes.Add(new DiffEntry
            {
                Kind = DiffKind.Removed,
                Path = $"{basePath}.{key}",
                OldValue = FormatParameterValue(baseline[key])
            });
        }
    }

    private static void CompareChannels(DeviceDiff diff, List<Channel> baseline, List<Channel> current)
    {
        var baselineByNumber = baseline.ToDictionary(c => c.Number);
        var currentByNumber = current.ToDictionary(c => c.Number);

        foreach (var (number, channel) in currentByNumber)
        {
            if (baselineByNumber.TryGetValue(number, out var baseChannel))
            {
                CompareParameters(diff, $"channels[{number}].parameters", baseChannel.Parameters, channel.Parameters);
            }
            else
            {
                diff.Changes.Add(new DiffEntry
                {
                    Kind = DiffKind.Added,
                    Path = $"channels[{number}]",
                    NewValue = channel.Name
                });
            }
        }

        foreach (var number in baselineByNumber.Keys.Where(n => !currentByNumber.ContainsKey(n)))
        {
            diff.Changes.Add(new DiffEntry
            {
                Kind = DiffKind.Removed,
                Path = $"channels[{number}]",
                OldValue = baselineByNumber[number].Name
            });
        }
    }

    private static void CompareRelations(DeviceDiff diff, List<Relation> baseline, List<Relation> current)
    {
        var baselineById = baseline.ToDictionary(r => r.Id);
        var currentById = current.ToDictionary(r => r.Id);

        foreach (var (id, relation) in currentById)
        {
            if (!baselineById.ContainsKey(id))
            {
                diff.Changes.Add(new DiffEntry
                {
                    Kind = DiffKind.Added,
                    Path = $"relations.{id}",
                    NewValue = $"{relation.Type} -> {relation.TargetId}"
                });
            }
        }

        foreach (var id in baselineById.Keys.Where(i => !currentById.ContainsKey(i)))
        {
            var relation = baselineById[id];
            diff.Changes.Add(new DiffEntry
            {
                Kind = DiffKind.Removed,
                Path = $"relations.{id}",
                OldValue = $"{relation.Type} -> {relation.TargetId}"
            });
        }
    }

    private static string FormatParameterValue(ParameterValue pv) => pv.Kind switch
    {
        ParameterKind.String => pv.StringValue ?? "",
        ParameterKind.Number => pv.NumberValue?.ToString() ?? "",
        ParameterKind.Boolean => pv.BoolValue?.ToString() ?? "",
        ParameterKind.Enum => pv.StringValue ?? "",
        _ => ""
    };
}
