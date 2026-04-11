using CreativeCoders.Core;
using Spectre.Console;

namespace SmartHal.CLI.Infrastructure;

/// <summary>
/// Console-based implementation of <see cref="IUserInteraction"/> using Spectre.Console.
/// </summary>
public class ConsoleUserInteraction(IAnsiConsole console) : IUserInteraction
{
    private readonly IAnsiConsole _console = Ensure.NotNull(console);

    /// <inheritdoc />
    public bool Confirm(string message, bool defaultYes = false)
    {
        return _console.Confirm(message, defaultYes);
    }

    /// <inheritdoc />
    public string? ReadLine(string prompt)
    {
        return _console.Prompt(new TextPrompt<string>(prompt).AllowEmpty());
    }

    /// <inheritdoc />
    public string ReadSecret(string prompt)
    {
        return _console.Prompt(new TextPrompt<string>(prompt).Secret());
    }
}
