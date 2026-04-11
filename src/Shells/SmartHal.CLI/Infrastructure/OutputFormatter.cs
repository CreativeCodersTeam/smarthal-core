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
public class OutputFormatter(CliContext context, IAnsiConsole console)
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
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
                console.WriteLine(JsonSerializer.Serialize(items, JsonOptions));
                break;
            case OutputFormat.Yaml:
                console.Write(new Text(YamlSerializer.Serialize(items)));
                break;
            case OutputFormat.Table:
                RenderTable(items, columns);
                break;
            default:
                throw new NotSupportedException($"Unsupported output format: {context.OutputFormat}");
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
                console.WriteLine(JsonSerializer.Serialize(item, JsonOptions));
                break;
            case OutputFormat.Yaml:
                console.Write(new Text(YamlSerializer.Serialize(item)));
                break;
            default:
                RenderProperties(properties);
                break;
        }
    }

    /// <summary>
    /// Writes a success/info message.
    /// </summary>
    /// <param name="message">The message text.</param>
    public void WriteSuccess(string message) =>
        console.MarkupLine($"[green]  {Markup.Escape(message)}[/]");

    /// <summary>
    /// Writes an error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public void WriteError(string message) =>
        console.MarkupLine($"[red]Error: {Markup.Escape(message)}[/]");

    /// <summary>
    /// Writes a warning message.
    /// </summary>
    /// <param name="message">The warning message.</param>
    public void WriteWarning(string message) =>
        console.MarkupLine($"[yellow]Warning: {Markup.Escape(message)}[/]");

    /// <summary>
    /// Writes validation results (errors and warnings).
    /// </summary>
    /// <param name="errors">The error messages with optional codes.</param>
    /// <param name="warnings">The warning messages with optional codes.</param>
    public void WriteValidationResults(
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
        console.MarkupLine($"Validation: {Markup.Escape(string.Join(", ", summary))}");
        console.WriteLine();

        if (errors.Count > 0)
        {
            console.MarkupLine("[red]  Errors:[/]");
            for (var i = 0; i < errors.Count; i++)
            {
                var (code, message, filePath) = errors[i];
                console.MarkupLine($"[red]    {i + 1}. [[{Markup.Escape(code)}]] {Markup.Escape(message)}[/]");
                if (filePath is not null)
                {
                    console.MarkupLine($"[red]       File: {Markup.Escape(filePath)}[/]");
                }
            }

            console.WriteLine();
        }

        if (warnings.Count > 0)
        {
            console.MarkupLine("[yellow]  Warnings:[/]");
            for (var i = 0; i < warnings.Count; i++)
            {
                var (code, message, filePath) = warnings[i];
                console.MarkupLine($"[yellow]    {i + 1}. [[{Markup.Escape(code)}]] {Markup.Escape(message)}[/]");
                if (filePath is not null)
                {
                    console.MarkupLine($"[yellow]       File: {Markup.Escape(filePath)}[/]");
                }
            }
        }
    }

    private void RenderTable<T>(IReadOnlyList<T> items, (string Header, Func<T, string> Value)[] columns)
    {
        if (items.Count == 0)
        {
            console.MarkupLine("[dim]No items found.[/]");
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

        console.Write(table);
    }

    private void RenderProperties((string Label, string Value)[] properties)
    {
        var maxLabel = properties.Max(p => p.Label.Length);

        foreach (var (label, value) in properties)
        {
            console.MarkupLine($"  [blue]{Markup.Escape(label.PadRight(maxLabel))}[/]  {Markup.Escape(value)}");
        }
    }
}
