using SmartHal.CLI.Infrastructure;
using AwesomeAssertions;

namespace SmartHal.CLI.Infrastructure;

public class OutputFormatterTests
{
    [Fact]
    public void WriteTable_JsonFormat_WritesValidJson()
    {
        var context = new CliContext { OutputFormat = OutputFormat.Json };
        var formatter = new OutputFormatter(context);
        var items = new List<TestItem>
        {
            new() { Name = "Alpha", Value = 1 },
            new() { Name = "Beta", Value = 2 }
        };

        var output = CaptureStdout(() =>
            formatter.WriteTable(
                items,
                ("Name", i => i.Name),
                ("Value", i => i.Value.ToString())));

        output.Should().Contain("\"name\"");
        output.Should().Contain("\"Alpha\"");
        output.Should().Contain("\"Beta\"");
    }

    [Fact]
    public void WriteTable_YamlFormat_WritesYaml()
    {
        var context = new CliContext { OutputFormat = OutputFormat.Yaml };
        var formatter = new OutputFormatter(context);
        var items = new List<TestItem>
        {
            new() { Name = "Alpha", Value = 1 }
        };

        var output = CaptureStdout(() =>
            formatter.WriteTable(
                items,
                ("Name", i => i.Name),
                ("Value", i => i.Value.ToString())));

        // YamlDotNet serializes list items with "- " prefix and underscore naming
        output.Should().Contain("Alpha");
        output.Should().Contain("1");
    }

    [Fact]
    public void WriteTable_TableFormat_EmptyList_WritesNoItemsMessage()
    {
        var context = new CliContext { OutputFormat = OutputFormat.Table };
        var formatter = new OutputFormatter(context);

        var stderr = CaptureStderr(() =>
            formatter.WriteTable(
                new List<TestItem>(),
                ("Name", i => i.Name)));

        stderr.Should().Contain("No items found");
    }

    [Fact]
    public void WriteObject_JsonFormat_WritesValidJson()
    {
        var context = new CliContext { OutputFormat = OutputFormat.Json };
        var formatter = new OutputFormatter(context);
        var item = new TestItem { Name = "Gamma", Value = 42 };

        var output = CaptureStdout(() =>
            formatter.WriteObject(
                item,
                ("Name", "Gamma"),
                ("Value", "42")));

        output.Should().Contain("\"name\"");
        output.Should().Contain("\"Gamma\"");
        output.Should().Contain("42");
    }

    [Fact]
    public void WriteObject_TableFormat_WritesLabelValuePairs()
    {
        var context = new CliContext { OutputFormat = OutputFormat.Table };
        var formatter = new OutputFormatter(context);
        var item = new TestItem { Name = "Delta", Value = 7 };

        var output = CaptureStdout(() =>
            formatter.WriteObject(
                item,
                ("Name", "Delta"),
                ("Value", "7")));

        output.Should().Contain("Name");
        output.Should().Contain("Delta");
        output.Should().Contain("Value");
        output.Should().Contain("7");
    }

    [Fact]
    public void WriteValidationResults_ErrorsAndWarnings_FormatsCorrectly()
    {
        var errors = new List<(string Code, string Message, string? FilePath)>
        {
            ("CFG-001", "Missing adapter reference", "devices/test.yaml")
        };
        var warnings = new List<(string Code, string Message, string? FilePath)>
        {
            ("CFG-W01", "Empty room", null)
        };

        var stderr = CaptureStderr(() =>
            OutputFormatter.WriteValidationResults(errors, warnings));

        stderr.Should().Contain("1 error(s)");
        stderr.Should().Contain("1 warning(s)");
        stderr.Should().Contain("[CFG-001]");
        stderr.Should().Contain("Missing adapter reference");
        stderr.Should().Contain("devices/test.yaml");
        stderr.Should().Contain("[CFG-W01]");
        stderr.Should().Contain("Empty room");
    }

    [Fact]
    public void WriteValidationResults_NoIssues_WritesNothing()
    {
        var stderr = CaptureStderr(() =>
            OutputFormatter.WriteValidationResults([], []));

        stderr.Should().BeEmpty();
    }

    private static string CaptureStdout(Action action)
    {
        var original = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            action();
            return writer.ToString();
        }
        finally
        {
            Console.SetOut(original);
        }
    }

    private static string CaptureStderr(Action action)
    {
        var original = Console.Error;
        using var writer = new StringWriter();
        Console.SetError(writer);
        try
        {
            action();
            return writer.ToString();
        }
        finally
        {
            Console.SetError(original);
        }
    }

    private class TestItem
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
