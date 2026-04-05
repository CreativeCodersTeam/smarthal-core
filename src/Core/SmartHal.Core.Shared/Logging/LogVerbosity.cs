namespace SmartHal.Core.Logging;

/// <summary>
/// Defines the verbosity levels for log output.
/// </summary>
public enum LogVerbosity
{
    /// <summary>Only warnings and errors are logged.</summary>
    Quiet,

    /// <summary>Default level — informational messages and above.</summary>
    Normal,

    /// <summary>Debug-level output, enabled with -v flag.</summary>
    Debug,

    /// <summary>Most detailed output, enabled with -vv flag.</summary>
    Verbose
}
