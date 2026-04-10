using AwesomeAssertions;

namespace SmartHal.Core.Secrets;

public class EncryptedFileSecretsProviderTests : IDisposable
{
    private readonly string _testFilePath;

    public EncryptedFileSecretsProviderTests()
    {
        _testFilePath = Path.Combine(Path.GetTempPath(), $"smarthal-test-{Guid.NewGuid()}.enc");
    }

    public void Dispose()
    {
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }
    }

    [Fact]
    public void ProviderName_IsFile()
    {
        var provider = CreateProvider("password");

        provider.ProviderName.Should().Be("file");
    }

    [Fact]
    public async Task Roundtrip_SetAndGet_ReturnsSameValue()
    {
        var provider = CreateProvider("password");

        await provider.SetSecretAsync("my.key", "my-secret-value");
        var value = await provider.GetSecretAsync("my.key");

        value.Should().Be("my-secret-value");
    }

    [Fact]
    public async Task GetSecretAsync_MissingKey_ThrowsSecretNotFoundException()
    {
        var provider = CreateProvider("password");

        var act = () => provider.GetSecretAsync("nonexistent.key");

        await act.Should().ThrowAsync<SmartHalSecretNotFoundException>();
    }

    [Fact]
    public async Task DeleteSecretAsync_RemovesKey()
    {
        var provider = CreateProvider("password");

        await provider.SetSecretAsync("delete.me", "value");
        await provider.DeleteSecretAsync("delete.me");
        var exists = await provider.ExistsAsync("delete.me");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ListKeysAsync_ReturnsAllKeys()
    {
        var provider = CreateProvider("password");

        await provider.SetSecretAsync("key.one", "v1");
        await provider.SetSecretAsync("key.two", "v2");
        var keys = await provider.ListKeysAsync();

        keys.Should().HaveCount(2);
        keys.Should().Contain("key.one");
        keys.Should().Contain("key.two");
    }

    [Fact]
    public async Task ExistsAsync_ExistingKey_ReturnsTrue()
    {
        var provider = CreateProvider("password");

        await provider.SetSecretAsync("exists.key", "value");
        var exists = await provider.ExistsAsync("exists.key");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_MissingKey_ReturnsFalse()
    {
        var provider = CreateProvider("password");

        var exists = await provider.ExistsAsync("missing.key");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task FileIsEncrypted_NotReadableAsPlaintext()
    {
        var provider = CreateProvider("password");

        await provider.SetSecretAsync("secret.key", "secret-value");

        var fileContent = await File.ReadAllTextAsync(_testFilePath);
        fileContent.Should().NotContain("secret-value");
        fileContent.Should().NotContain("secret.key");
    }

    [Fact]
    public async Task WrongPassword_ThrowsSecretsProviderException()
    {
        var provider = CreateProvider("correct-password");
        await provider.SetSecretAsync("my.key", "value");

        var wrongProvider = CreateProvider("wrong-password");
        var act = () => wrongProvider.GetSecretAsync("my.key");

        await act.Should().ThrowAsync<SmartHalSecretsProviderException>();
    }

    [Fact]
    public async Task MultipleSecrets_CanBeStoredAndRetrieved()
    {
        var provider = CreateProvider("password");

        await provider.SetSecretAsync("adapter1.api_key", "key1");
        await provider.SetSecretAsync("adapter2.api_key", "key2");
        await provider.SetSecretAsync("adapter3.token", "tok3");

        (await provider.GetSecretAsync("adapter1.api_key")).Should().Be("key1");
        (await provider.GetSecretAsync("adapter2.api_key")).Should().Be("key2");
        (await provider.GetSecretAsync("adapter3.token")).Should().Be("tok3");
    }

    private EncryptedFileSecretsProvider CreateProvider(string password) =>
        new EncryptedFileSecretsProvider(() => Task.FromResult(password), _testFilePath);
}
