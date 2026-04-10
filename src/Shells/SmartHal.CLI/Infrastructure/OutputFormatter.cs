using System.Text.Json;
using System.Text.Json.Serialization;
using Spectre.Console;
using Spectre.Console.Rendering;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SmartHal.CLI.Infrastructure;

/// <summary>
/// Formats command output to stdout in the requested format (Table, JSON, YAML).
/// </summary>
public class OutputFormatter(CliContext context)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) }
    };

    private static readonly ISerializer YamlSerializer = new SerializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
        .Build();

    /// <summary>
    /// Writes a collection of items in the configured output format.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="items">The items to display.</param>
    /// <param name="columns">Column definitions for table format (header, value selector).</param>
    public void WriteTable<T>(IReadOnlyList<T> items, params (string Header, Func<T, string> Value)[] columns)
    {
        switch (context.OutputFormat)
        {
            case OutputFormat.Json:
                Console.WriteLine(JsonSerializer.Serialize(items, JsonOptions));
                break;
            case OutputFormat.Yaml:
                Console.Write(YamlSerializer.Serialize(items));
                break;
            default:
                RenderTable(items, columns);
                break;
        }
    }

    /// <summary>
    /// Writes a single object in the configured output format.
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    /// <param name="item">The item to display.</param>
    /// <param name="properties">Property definitions for table format (label, value).</param>
    public void WriteObject<T>(T item, params (string Label, string Value)[] properties)
    {
        switch (context.OutputFormat)
        {
            case OutputFormat.Json:
                Console.WriteLine(JsonSerializer.Serialize(item, JsonOptions));
                break;
            case OutputFormat.Yaml:
                Console.Write(YamlSerializer.Serialize(item));
                break;
            default:
                RenderProperties(properties);
                break;
        }
    }

    /// <summary>
    /// Writes a simple message to stdout.
    /// </summary>
    /// <param name="message">The message text.</param>
    public static void WriteSuccess(string message) =>
        Console.Error.WriteLine($"  {message}");

    /// <summary>
    /// Writes an error message to stderr.
    /// </summary>
    /// <param name="message">The error message.</param>
    public static void WriteError(string message) =>
        Console.Error.WriteLine($"Error: {message}");

    /// <summary>
    /// Writes a warning message to stderr.
    /// </summary>
    /// <param name="message">The warning message.</param>
    public static void WriteWarning(string message) =>
        Console.Error.WriteLine($"Warning: {message}");

    /// <summary>
    /// Writes validation results (errors and warnings) to stderr.
    /// </summary>
    /// <param name="errors">The error messages with optional codes.</param>
    /// <param name="warnings">The warning messages with optional codes.</param>
    public static void WriteValidationResults(
        IReadOnlyList<(string Code, string Message, string? FilePath)> errors,
        IReadOnlyList<(string Code, string Message, string? FilePath)> warnings)
    {
        if (errors.Count == 0 && warnings.Count == 0)
        {
            return;
        }

        var summary = new List<string>();
        if (errors.Count > 0) summary.Add($"{errors.Count} error(s)");
        if (warnings.Count > 0) summary.Add($"{warnings.Count} warning(s)");
        Console.Error.WriteLine($"Validation: {string.Join(", ", summary)}");
        Console.Error.WriteLine();

        if (errors.Count > 0)
        {
            Console.Error.WriteLine("  Errors:");
            for (var i = 0; i < errors.Count; i++)
            {
                var (code, message, filePath) = errors[i];
                Console.Error.WriteLine($"    {i + 1}. [{code}] {message}");
                if (filePath is not null)
                {
                    Console.Error.WriteLine($"       File: {filePath}");
                }
            }

            Console.Error.WriteLine();
        }

        if (warnings.Count > 0)
        {
            Console.Error.WriteLine("  Warnings:");
            for (var i = 0; i < warnings.Count; i++)
            {
                var (code, message, filePath) = warnings[i];
                Console.Error.WriteLine($"    {i + 1}. [{code}] {message}");
                if (filePath is not null)
                {
                    Console.Error.WriteLine($"       File: {filePath}");
                }
            }
        }
    }

    private static void RenderTable<T>(IReadOnlyList<T> items, (string Header, Func<T, string> Value)[] columns)
    {
        if (items.Count == 0)
        {
            Console.Error.WriteLine("No items found.");
            return;
        }

        var table = new Table();
        table.Border(TableBorder.Simple);

        foreach (var (header, _) in columns)
        {
            table.AddColumn(header);
        }

        foreach (var item in items)
        {
            var values = columns.Select(c => c.Value(item)).ToArray();
            table.AddRow(values.Select(v => new Text(v)).ToArray<IRenderable>());
        }

        AnsiConsole.Write(table);
    }

    private static void RenderProperties((string Label, string Value)[] properties)
    {
        var maxLabel = properties.Max(p => p.Label.Length);

        foreach (var (label, value) in properties)
        {
            Console.WriteLine($"  {label.PadRight(maxLabel)}  {value}");
        }
    }
}
