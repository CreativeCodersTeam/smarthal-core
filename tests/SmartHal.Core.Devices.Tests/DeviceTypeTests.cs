using AwesomeAssertions;

namespace SmartHal.Core.Devices;

public class DeviceTypeTests
{
    [Fact]
    public void StaticConstants_HaveCorrectValues()
    {
        DeviceType.BlindActuator.Value.Should().Be("blind_actuator");
        DeviceType.SwitchActuator.Value.Should().Be("switch_actuator");
        DeviceType.DimmerActuator.Value.Should().Be("dimmer_actuator");
        DeviceType.Wallbox.Value.Should().Be("wallbox");
        DeviceType.MqttSensor.Value.Should().Be("mqtt_sensor");
        DeviceType.Unknown.Value.Should().Be("unknown");
    }

    [Fact]
    public void ImplicitConversion_ToString()
    {
        string value = DeviceType.BlindActuator;

        value.Should().Be("blind_actuator");
    }

    [Fact]
    public void ImplicitConversion_FromString()
    {
        DeviceType type = "custom_type";

        type.Value.Should().Be("custom_type");
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = new DeviceType("blind_actuator");
        var b = DeviceType.BlindActuator;

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentValue_AreNotEqual()
    {
        DeviceType.BlindActuator.Should().NotBe(DeviceType.SwitchActuator);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        DeviceType.Wallbox.ToString().Should().Be("wallbox");
    }

    [Fact]
    public void CustomType_CanBeCreated()
    {
        var custom = new DeviceType("my_custom_sensor");

        custom.Value.Should().Be("my_custom_sensor");
    }
}
