using System.Runtime.CompilerServices;

namespace SmartHal.Adapters.HomeMatic;

/// <summary>
/// Marker type used to reference the HomeMatic adapter assembly for adapter discovery.
/// </summary>
public static class AssemblyReference
{
    /// <summary>Gets the assembly containing the HomeMatic adapter implementations.</summary>
    public static System.Reflection.Assembly Assembly => typeof(AssemblyReference).Assembly;
}
