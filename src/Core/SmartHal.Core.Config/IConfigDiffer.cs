using SmartHal.Core.Devices;

namespace SmartHal.Core.Config;

/// <summary>
/// Compares two device states and produces a list of changes.
/// </summary>
public interface IConfigDiffer
{
    /// <summary>Computes the diff between a baseline and the current state of a device.</summary>
    DeviceDiff ComputeDiff(Device baseline, Device current);
}
