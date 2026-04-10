using AwesomeAssertions;
using SmartHal.Core.Adapters.Fakes;

namespace SmartHal.Core.Adapters;

public class AdapterFactoryTests
{
    [Fact]
    public void GetAvailableAdapterTypes_WithNoRegistrations_ReturnsEmpty()
    {
        // Arrange
        var factory = new AdapterFactory();

        // Act & Assert
        factory.GetAvailableAdapterTypes().Should().BeEmpty();
    }

    [Fact]
    public void RegisterAssembly_WithAdapterMetadata_RegistersType()
    {
        // Arrange
        var factory = new AdapterFactory();

        // Act
        factory.RegisterAssembly(typeof(FakeAdapter).Assembly);

        // Assert
        factory.GetAvailableAdapterTypes().Should().HaveCount(1);
        factory.GetAvailableAdapterTypes().Should().Contain("fake");
    }

    [Fact]
    public void CreateAdapter_WithKnownType_ReturnsInstance()
    {
        // Arrange
        var factory = new AdapterFactory();
        factory.RegisterAssembly(typeof(FakeAdapter).Assembly);
        var config = new AdapterConfig { AdapterId = "fake-001", AdapterType = "fake" };

        // Act
        var adapter = factory.CreateAdapter(config);

        // Assert
        adapter.Should().NotBeNull();
        adapter.Should().BeAssignableTo<ISmartHalAdapter>();
    }

    [Fact]
    public void RegisterAssembly_DuplicateAdapterType_ThrowsSmartHalAdapterException()
    {
        // Arrange
        var factory = new AdapterFactory();
        factory.RegisterAssembly(typeof(FakeAdapter).Assembly);

        // Act
        var act = () => factory.RegisterAssembly(typeof(FakeAdapter).Assembly);

        // Assert
        act.Should().Throw<SmartHalAdapterException>();
    }

    [Fact]
    public void CreateAdapter_WithUnknownType_ThrowsSmartHalAdapterException()
    {
        // Arrange
        var factory = new AdapterFactory();
        var config = new AdapterConfig { AdapterId = "test-001", AdapterType = "nonexistent" };

        // Act
        var act = () => factory.CreateAdapter(config);

        // Assert
        act.Should().Throw<SmartHalAdapterException>()
            .Which.AdapterId.Should().Be("test-001");
    }
}
