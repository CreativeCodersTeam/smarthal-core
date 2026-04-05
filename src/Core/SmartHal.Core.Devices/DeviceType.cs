namespace SmartHal.Core.Devices;

/// <summary>
/// Represents the type of a device. Implemented as a readonly record struct
/// with well-known constants to allow adapter-specific extensions.
/// </summary>
public readonly record struct DeviceType(string Value)
{
    public static readonly DeviceType BlindActuator = new DeviceType("blind_actuator");
    public static readonly DeviceType SwitchActuator = new DeviceType("switch_actuator");
    public static readonly DeviceType DimmerActuator = new DeviceType("dimmer_actuator");
    public static readonly DeviceType Wallbox = new DeviceType("wallbox");
    public static readonly DeviceType MqttSensor = new DeviceType("mqtt_sensor");
    public static readonly DeviceType Unknown = new DeviceType("unknown");

    /// <inheritdoc />
    public override string ToString() => Value;

    public static implicit operator string(DeviceType t) => t.Value;
    public static implicit operator DeviceType(string v) => new DeviceType(v);
}
