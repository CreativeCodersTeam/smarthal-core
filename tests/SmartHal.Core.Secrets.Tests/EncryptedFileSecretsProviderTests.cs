using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;

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
        // Arrange
        var provider = CreateProvider("password");

        // Act & Assert
        provider.ProviderName.Should().Be("file");
    }

    [Fact]
    public async Task Roundtrip_SetAndGet_ReturnsSameValue()
    {
        // Arrange
        var provider = CreateProvider("password");

        // Act
        await provider.SetSecretAsync("my.key", "my-secret-value");
        var value = await provider.GetSecretAsync("my.key");

        // Assert
        value.Should().Be("my-secret-value");
    }

    [Fact]
    public async Task GetSecretAsync_MissingKey_ThrowsSecretNotFoundException()
    {
        // Arrange
        var provider = CreateProvider("password");

        // Act
        var act = () => provider.GetSecretAsync("nonexistent.key");

        // Assert
        await act.Should().ThrowAsync<SmartHalSecretNotFoundException>();
    }

    [Fact]
    public async Task DeleteSecretAsync_RemovesKey()
    {
        // Arrange
        var provider = CreateProvider("password");
        await provider.SetSecretAsync("delete.me", "value");

        // Act
        await provider.DeleteSecretAsync("delete.me");
        var exists = await provider.ExistsAsync("delete.me");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ListKeysAsync_ReturnsAllKeys()
    {
        // Arrange
        var provider = CreateProvider("password");
        await provider.SetSecretAsync("key.one", "v1");
        await provider.SetSecretAsync("key.two", "v2");

        // Act
        var keys = await provider.ListKeysAsync();

        // Assert
        keys.Should().HaveCount(2);
        keys.Should().Contain("key.one");
        keys.Should().Contain("key.two");
    }

    [Fact]
    public async Task ExistsAsync_ExistingKey_ReturnsTrue()
    {
        // Arrange
        var provider = CreateProvider("password");
        await provider.SetSecretAsync("exists.key", "value");

        // Act
        var exists = await provider.ExistsAsync("exists.key");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_MissingKey_ReturnsFalse()
    {
        // Arrange
        var provider = CreateProvider("password");

        // Act
        var exists = await provider.ExistsAsync("missing.key");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task FileIsEncrypted_NotReadableAsPlaintext()
    {
        // Arrange
        var provider = CreateProvider("password");
        await provider.SetSecretAsync("secret.key", "secret-value");

        // Act
        var fileContent = await File.ReadAllTextAsync(_testFilePath);

        // Assert
        fileContent.Should().NotContain("secret-value");
        fileContent.Should().NotContain("secret.key");
    }

    [Fact]
    public async Task WrongPassword_ThrowsSecretsProviderException()
    {
        // Arrange
        var provider = CreateProvider("correct-password");
        await provider.SetSecretAsync("my.key", "value");
        var wrongProvider = CreateProvider("wrong-password");

        // Act
        var act = () => wrongProvider.GetSecretAsync("my.key");

        // Assert
        await act.Should().ThrowAsync<SmartHalSecretsProviderException>();
    }

    [Fact]
    public async Task MultipleSecrets_CanBeStoredAndRetrieved()
    {
        // Arrange
        var provider = CreateProvider("password");
        await provider.SetSecretAsync("adapter1.api_key", "key1");
        await provider.SetSecretAsync("adapter2.api_key", "key2");
        await provider.SetSecretAsync("adapter3.token", "tok3");

        // Act & Assert
        (await provider.GetSecretAsync("adapter1.api_key")).Should().Be("key1");
        (await provider.GetSecretAsync("adapter2.api_key")).Should().Be("key2");
        (await provider.GetSecretAsync("adapter3.token")).Should().Be("tok3");
    }

    [Fact]
    public async Task SetSecretAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        // Arrange
        var provider = CreateProvider("password");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var act = () => provider.SetSecretAsync("key", "value", cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task SetSecretAsync_ConcurrentWrites_AllSucceed()
    {
        // Arrange
        var provider = CreateProvider("password");

        // Act
        var tasks = Enumerable.Range(1, 5)
            .Select(i => provider.SetSecretAsync($"key{i}", $"value{i}"))
            .ToArray();
        await Task.WhenAll(tasks);

        // Assert
        var keys = await provider.ListKeysAsync();
        keys.Should().HaveCount(5);
    }

    private EncryptedFileSecretsProvider CreateProvider(string password) =>
        new EncryptedFileSecretsProvider(
            () => Task.FromResult(password),
            NullLogger<EncryptedFileSecretsProvider>.Instance,
            _testFilePath);
}
