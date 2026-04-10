using System.Text;

namespace SmartHal.CLI.Infrastructure;

/// <summary>
/// Console-based implementation of <see cref="IUserInteraction"/>.
/// </summary>
public class ConsoleUserInteraction : IUserInteraction
{
    /// <inheritdoc />
    public bool Confirm(string message, bool defaultYes = false)
    {
        var hint = defaultYes ? "[Y/n]" : "[y/N]";
        Console.Error.Write($"{message} {hint} ");

        var input = Console.ReadLine()?.Trim().ToLowerInvariant();

        return input switch
        {
            "y" or "j" or "yes" or "ja" => true,
            "n" or "no" or "nein" => false,
            "" or null => defaultYes,
            _ => false
        };
    }

    /// <inheritdoc />
    public string? ReadLine(string prompt)
    {
        Console.Error.Write(prompt);
        return Console.ReadLine();
    }

    /// <inheritdoc />
    public string ReadSecret(string prompt)
    {
        Console.Error.Write(prompt);

        var secret = new StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.Error.WriteLine();
                break;
            }

            if (key.Key == ConsoleKey.Backspace && secret.Length > 0)
            {
                secret.Remove(secret.Length - 1, 1);
            }
            else if (!char.IsControl(key.KeyChar))
            {
                secret.Append(key.KeyChar);
            }
        }

        return secret.ToString();
    }
}
