using Spectre.Console;

namespace SmartHal.CLI.Infrastructure;

/// <summary>
/// Console-based implementation of <see cref="IUserInteraction"/> using Spectre.Console.
/// </summary>
public class ConsoleUserInteraction(IAnsiConsole console) : IUserInteraction
{
    /// <inheritdoc />
    public bool Confirm(string message, bool defaultYes = false)
    {
        return console.Confirm(message, defaultYes);
    }

    /// <inheritdoc />
    public string? ReadLine(string prompt)
    {
        return console.Prompt(new TextPrompt<string>(prompt).AllowEmpty());
    }

    /// <inheritdoc />
    public string ReadSecret(string prompt)
    {
        return console.Prompt(new TextPrompt<string>(prompt).Secret());
    }
}
