namespace SmartHal.CLI.Infrastructure;

/// <summary>
/// Abstraction for user interaction (confirmations, input).
/// Allows mocking in tests.
/// </summary>
public interface IUserInteraction
{
    /// <summary>
    /// Asks the user for confirmation. Returns <c>true</c> if the user confirms.
    /// </summary>
    /// <param name="message">The prompt message (e.g. "Continue?").</param>
    /// <param name="defaultYes">Whether the default answer is yes.</param>
    /// <returns><c>true</c> if confirmed; otherwise <c>false</c>.</returns>
    bool Confirm(string message, bool defaultYes = false);

    /// <summary>
    /// Reads a line of input from the user.
    /// </summary>
    /// <param name="prompt">The prompt to display.</param>
    /// <returns>The user's input.</returns>
    string? ReadLine(string prompt);

    /// <summary>
    /// Reads a secret value from the user (hidden input).
    /// </summary>
    /// <param name="prompt">The prompt to display.</param>
    /// <returns>The secret value.</returns>
    string ReadSecret(string prompt);
}
