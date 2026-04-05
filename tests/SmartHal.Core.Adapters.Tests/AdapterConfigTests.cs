using AwesomeAssertions;

namespace SmartHal.Core.Adapters;

public class AdapterConfigTests
{
    [Fact]
    public void DefaultInitialization_HasEmptyValues()
    {
        var config = new AdapterConfig();

        config.AdapterId.Should().BeEmpty();
        config.AdapterType.Should().BeEmpty();
        config.Settings.Should().BeEmpty();
    }

    [Fact]
    public void Settings_CanBePopulated()
    {
        var config = new AdapterConfig
        {
            AdapterId = "hm-eg",
            AdapterType = "homematic",
            Settings =
            {
                ["host"] = "192.168.1.100",
                ["api_key"] = "vault:homematic-api-key"
            }
        };

        config.Settings.Should().HaveCount(2);
        config.Settings["host"].Should().Be("192.168.1.100");
    }
}
