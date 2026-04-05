using AwesomeAssertions;

namespace SmartHal.Core.Exceptions;

public class SmartHalAdapterExceptionTests
{
    [Fact]
    public void AdapterException_StoresAdapterId()
    {
        var ex = new SmartHalAdapterException("error", "homematic-001");

        ex.AdapterId.Should().Be("homematic-001");
    }

    [Fact]
    public void AdapterException_WithInnerException_SetsBoth()
    {
        var inner = new InvalidOperationException("inner");

        var ex = new SmartHalAdapterException("error", "homematic-001", inner);

        ex.AdapterId.Should().Be("homematic-001");
        ex.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void AdapterConnectionException_DerivedFromAdapterException()
    {
        var ex = new SmartHalAdapterConnectionException("connection failed", "zigbee-001");

        ex.Should().BeAssignableTo<SmartHalAdapterException>();
        ex.AdapterId.Should().Be("zigbee-001");
    }

    [Fact]
    public void AdapterDeviceNotFoundException_StoresNativeId()
    {
        var ex = new SmartHalAdapterDeviceNotFoundException("not found", "homematic-001", "HM-1234");

        ex.AdapterId.Should().Be("homematic-001");
        ex.NativeId.Should().Be("HM-1234");
    }

    [Fact]
    public void AdapterOperationException_WithInnerException_SetsBoth()
    {
        var inner = new TimeoutException("timeout");

        var ex = new SmartHalAdapterOperationException("operation failed", "knx-001", inner);

        ex.AdapterId.Should().Be("knx-001");
        ex.InnerException.Should().BeSameAs(inner);
    }
}
