using SmartHal.Core.Devices;

namespace SmartHal.Core.Config;

/// <summary>
/// Compares two device states and produces a list of changes.
/// </summary>
public interface IConfigDiffer
{
    /// <summary>Computes the diff between a baseline and the current state of a device.</summary>
    /// <param name="baseline">The baseline device state.</param>
    /// <param name="current">The current device state.</param>
    /// <returns>The diff between the two device states.</returns>
    DeviceDiff ComputeDiff(Device baseline, Device current);
}
