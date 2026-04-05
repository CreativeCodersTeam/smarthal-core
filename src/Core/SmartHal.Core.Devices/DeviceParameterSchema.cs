namespace SmartHal.Core.Devices;

/// <summary>
/// Schema definition for device parameters that require special handling,
/// such as enum fields with a finite set of adapter-specific values.
/// </summary>
public class DeviceParameterSchema : Dictionary<string, ParameterKind>;
