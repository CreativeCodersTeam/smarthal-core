using AwesomeAssertions;

namespace SmartHal.Core.Exceptions;

public class SmartHalAdapterExceptionTests
{
    [Fact]
    public void AdapterException_StoresAdapterId()
    {
        // Act
        var ex = new SmartHalAdapterException("error", "homematic-001");

        // Assert
        ex.AdapterId.Should().Be("homematic-001");
    }

    [Fact]
    public void AdapterException_WithInnerException_SetsBoth()
    {
        // Arrange
        var inner = new InvalidOperationException("inner");

        // Act
        var ex = new SmartHalAdapterException("error", "homematic-001", inner);

        // Assert
        ex.AdapterId.Should().Be("homematic-001");
        ex.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void AdapterConnectionException_DerivedFromAdapterException()
    {
        // Act
        var ex = new SmartHalAdapterConnectionException("connection failed", "zigbee-001");

        // Assert
        ex.Should().BeAssignableTo<SmartHalAdapterException>();
        ex.AdapterId.Should().Be("zigbee-001");
    }

    [Fact]
    public void AdapterDeviceNotFoundException_StoresNativeId()
    {
        // Act
        var ex = new SmartHalAdapterDeviceNotFoundException("not found", "homematic-001", "HM-1234");

        // Assert
        ex.AdapterId.Should().Be("homematic-001");
        ex.NativeId.Should().Be("HM-1234");
    }

    [Fact]
    public void AdapterOperationException_WithInnerException_SetsBoth()
    {
        // Arrange
        var inner = new TimeoutException("timeout");

        // Act
        var ex = new SmartHalAdapterOperationException("operation failed", "knx-001", inner);

        // Assert
        ex.AdapterId.Should().Be("knx-001");
        ex.InnerException.Should().BeSameAs(inner);
    }
}
