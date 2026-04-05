using AwesomeAssertions;
using SmartHal.Core.Adapters.Fakes;

namespace SmartHal.Core.Adapters;

public class AdapterFactoryTests
{
    [Fact]
    public void GetAvailableAdapterTypes_WithNoRegistrations_ReturnsEmpty()
    {
        var factory = new AdapterFactory();

        factory.GetAvailableAdapterTypes().Should().BeEmpty();
    }

    [Fact]
    public void RegisterAssembly_WithAdapterMetadata_RegistersType()
    {
        var factory = new AdapterFactory();

        factory.RegisterAssembly(typeof(FakeAdapter).Assembly);

        factory.GetAvailableAdapterTypes().Should().HaveCount(1);
        factory.GetAvailableAdapterTypes().Should().Contain("fake");
    }

    [Fact]
    public void CreateAdapter_WithKnownType_ReturnsInstance()
    {
        var factory = new AdapterFactory();
        factory.RegisterAssembly(typeof(FakeAdapter).Assembly);

        var config = new AdapterConfig { AdapterId = "fake-001", AdapterType = "fake" };

        var adapter = factory.CreateAdapter(config);

        adapter.Should().NotBeNull();
        adapter.Should().BeAssignableTo<ISmartHalAdapter>();
    }

    [Fact]
    public void RegisterAssembly_DuplicateAdapterType_ThrowsSmartHalAdapterException()
    {
        var factory = new AdapterFactory();
        factory.RegisterAssembly(typeof(FakeAdapter).Assembly);

        var act = () => factory.RegisterAssembly(typeof(FakeAdapter).Assembly);

        act.Should().Throw<SmartHalAdapterException>();
    }

    [Fact]
    public void CreateAdapter_WithUnknownType_ThrowsSmartHalAdapterException()
    {
        var factory = new AdapterFactory();

        var config = new AdapterConfig { AdapterId = "test-001", AdapterType = "nonexistent" };

        var act = () => factory.CreateAdapter(config);

        act.Should().Throw<SmartHalAdapterException>()
            .Which.AdapterId.Should().Be("test-001");
    }
}
