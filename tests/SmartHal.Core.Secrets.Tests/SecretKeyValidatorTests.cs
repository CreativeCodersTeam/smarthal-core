using AwesomeAssertions;

namespace SmartHal.Core.Secrets;

public class SecretKeyValidatorTests
{
    [Theory]
    [InlineData("homematic-eg.api_key")]
    [InlineData("zigbee.host")]
    [InlineData("ab")]
    [InlineData("a1.b2")]
    [InlineData("my-adapter.setting-name")]
    public void IsValid_ValidKeys_ReturnsTrue(string key)
    {
        SecretKeyValidator.IsValid(key).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("has space")]
    [InlineData("UPPERCASE")]
    [InlineData("special!char")]
    [InlineData(".starts-with-dot")]
    [InlineData("ends-with-dot.")]
    [InlineData("-starts-with-dash")]
    [InlineData("a")]
    public void IsValid_InvalidKeys_ReturnsFalse(string key)
    {
        SecretKeyValidator.IsValid(key).Should().BeFalse();
    }

    [Fact]
    public void IsValid_Null_ReturnsFalse()
    {
        SecretKeyValidator.IsValid(null!).Should().BeFalse();
    }

    [Theory]
    [InlineData("homematic-eg.api_key", "SMARTHAL_HOMEMATIC_EG_API_KEY")]
    [InlineData("zigbee.host", "SMARTHAL_ZIGBEE_HOST")]
    [InlineData("my-adapter.my-setting", "SMARTHAL_MY_ADAPTER_MY_SETTING")]
    public void ToEnvironmentVariable_ConvertsCorrectly(string key, string expected)
    {
        SecretKeyValidator.ToEnvironmentVariable(key).Should().Be(expected);
    }

    [Theory]
    [InlineData("SMARTHAL_HOMEMATIC_EG_API_KEY", "homematic_eg_api_key")]
    [InlineData("SMARTHAL_ZIGBEE_HOST", "zigbee_host")]
    public void FromEnvironmentVariable_WithPrefix_ReturnsKey(string envVar, string expected)
    {
        SecretKeyValidator.FromEnvironmentVariable(envVar).Should().Be(expected);
    }

    [Fact]
    public void FromEnvironmentVariable_WithoutPrefix_ReturnsNull()
    {
        SecretKeyValidator.FromEnvironmentVariable("OTHER_VAR").Should().BeNull();
    }
}
