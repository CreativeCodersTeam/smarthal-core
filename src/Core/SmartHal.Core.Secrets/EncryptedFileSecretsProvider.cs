using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SmartHal.Core.Secrets;

/// <summary>
/// Stores secrets in an AES-256-GCM encrypted JSON file.
/// The encryption key is derived from a master password via PBKDF2.
/// </summary>
public class EncryptedFileSecretsProvider : ISecretsProvider
{
    private const int SaltSize = 16;
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private const int KeySize = 32;
    private const int Pbkdf2Iterations = 100_000;
    private const int FileVersion = 1;

    private readonly string _filePath;
    private readonly Func<Task<string>> _passwordCallback;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private byte[]? _derivedKey;
    private byte[]? _salt;

    /// <inheritdoc />
    public string ProviderName => "file";

    /// <summary>
    /// Initializes a new instance of the <see cref="EncryptedFileSecretsProvider"/> class.
    /// </summary>
    /// <param name="passwordCallback">Callback to retrieve the master password when needed.</param>
    /// <param name="filePath">Path to the encrypted secrets file. Defaults to <c>~/.smarthal/secrets.enc</c>.</param>
    public EncryptedFileSecretsProvider(Func<Task<string>> passwordCallback, string? filePath = null)
    {
        _passwordCallback = passwordCallback;
        _filePath = filePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".smarthal", "secrets.enc");
    }

    /// <inheritdoc />
    public async Task<string> GetSecretAsync(string key, CancellationToken ct = default)
    {
        var secrets = await ReadSecretsAsync(ct).ConfigureAwait(false);

        return secrets.TryGetValue(key, out var value)
            ? value
            : throw new SmartHalSecretNotFoundException(key);
    }

    /// <inheritdoc />
    public async Task SetSecretAsync(string key, string value, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct).ConfigureAwait(false);

        try
        {
            var secrets = await ReadSecretsAsync(ct).ConfigureAwait(false);
            secrets[key] = value;
            await WriteSecretsAsync(secrets, ct).ConfigureAwait(false);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task DeleteSecretAsync(string key, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct).ConfigureAwait(false);

        try
        {
            var secrets = await ReadSecretsAsync(ct).ConfigureAwait(false);
            secrets.Remove(key);
            await WriteSecretsAsync(secrets, ct).ConfigureAwait(false);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> ListKeysAsync(CancellationToken ct = default)
    {
        var secrets = await ReadSecretsAsync(ct).ConfigureAwait(false);
        return secrets.Keys.ToList();
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        var secrets = await ReadSecretsAsync(ct).ConfigureAwait(false);
        return secrets.ContainsKey(key);
    }

    private async Task EnsureKeyDerivedAsync(CancellationToken ct)
    {
        if (_derivedKey is not null)
        {
            return;
        }

        var password = await _passwordCallback().ConfigureAwait(false);

        if (File.Exists(_filePath))
        {
            var fileBytes = await File.ReadAllBytesAsync(_filePath, ct).ConfigureAwait(false);
            _salt = fileBytes[..SaltSize];
        }
        else
        {
            _salt = RandomNumberGenerator.GetBytes(SaltSize);
        }

        _derivedKey = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            _salt,
            Pbkdf2Iterations,
            HashAlgorithmName.SHA256,
            KeySize);
    }

    private async Task<Dictionary<string, string>> ReadSecretsAsync(CancellationToken ct)
    {
        await EnsureKeyDerivedAsync(ct).ConfigureAwait(false);

        if (!File.Exists(_filePath))
        {
            return [];
        }

        var fileBytes = await File.ReadAllBytesAsync(_filePath, ct).ConfigureAwait(false);

        if (fileBytes.Length < SaltSize + NonceSize + TagSize)
        {
            return [];
        }

        var nonce = fileBytes[SaltSize..(SaltSize + NonceSize)];
        var tag = fileBytes[(SaltSize + NonceSize)..(SaltSize + NonceSize + TagSize)];
        var ciphertext = fileBytes[(SaltSize + NonceSize + TagSize)..];
        var plaintext = new byte[ciphertext.Length];

        try
        {
            using var aes = new AesGcm(_derivedKey!, TagSize);
            aes.Decrypt(nonce, ciphertext, tag, plaintext);
        }
        catch (CryptographicException ex)
        {
            throw new SmartHalSecretsProviderException(
                "Failed to decrypt secrets file. Wrong password?", ProviderName, ex);
        }

        var json = Encoding.UTF8.GetString(plaintext);
        var document = JsonSerializer.Deserialize<SecretsDocument>(json)
                       ?? new SecretsDocument();

        return document.Secrets;
    }

    private async Task WriteSecretsAsync(Dictionary<string, string> secrets, CancellationToken ct)
    {
        await EnsureKeyDerivedAsync(ct).ConfigureAwait(false);

        var document = new SecretsDocument { Version = FileVersion, Secrets = secrets };
        var json = JsonSerializer.Serialize(document);
        var plaintext = Encoding.UTF8.GetBytes(json);

        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_derivedKey!, TagSize);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);

        var directory = Path.GetDirectoryName(_filePath);
        if (directory is not null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // File format: [salt][nonce][tag][ciphertext]
        var output = new byte[SaltSize + NonceSize + TagSize + ciphertext.Length];
        _salt!.CopyTo(output, 0);
        nonce.CopyTo(output, SaltSize);
        tag.CopyTo(output, SaltSize + NonceSize);
        ciphertext.CopyTo(output, SaltSize + NonceSize + TagSize);

        await File.WriteAllBytesAsync(_filePath, output, ct).ConfigureAwait(false);
    }

    private sealed class SecretsDocument
    {
        public int Version { get; set; } = FileVersion;
        public Dictionary<string, string> Secrets { get; set; } = [];
    }
}
