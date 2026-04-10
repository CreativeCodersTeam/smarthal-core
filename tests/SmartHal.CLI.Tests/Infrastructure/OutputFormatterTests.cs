using AwesomeAssertions;
using Spectre.Console.Testing;

namespace SmartHal.CLI.Infrastructure;

public class OutputFormatterTests
{
    [Fact]
    public void WriteTable_JsonFormat_WritesValidJson()
    {
        var console = new TestConsole();
        var context = new CliContext { OutputFormat = OutputFormat.Json };
        var formatter = new OutputFormatter(context, console);
        var items = new List<TestItem>
        {
            new TestItem { Name = "Alpha", Value = 1 },
            new TestItem { Name = "Beta", Value = 2 }
        };

        formatter.WriteTable(
            items,
            ("Name", i => i.Name),
            ("Value", i => i.Value.ToString()));

        var output = console.Output;
        output.Should().Contain("\"name\"");
        output.Should().Contain("\"Alpha\"");
        output.Should().Contain("\"Beta\"");
    }

    [Fact]
    public void WriteTable_YamlFormat_WritesYaml()
    {
        var console = new TestConsole();
        var context = new CliContext { OutputFormat = OutputFormat.Yaml };
        var formatter = new OutputFormatter(context, console);
        var items = new List<TestItem>
        {
            new TestItem { Name = "Alpha", Value = 1 }
        };

        formatter.WriteTable(
            items,
            ("Name", i => i.Name),
            ("Value", i => i.Value.ToString()));

        var output = console.Output;
        // YamlDotNet serializes list items with "- " prefix and underscore naming
        output.Should().Contain("Alpha");
        output.Should().Contain("1");
    }

    [Fact]
    public void WriteTable_TableFormat_EmptyList_WritesNoItemsMessage()
    {
        var console = new TestConsole();
        var context = new CliContext { OutputFormat = OutputFormat.Table };
        var formatter = new OutputFormatter(context, console);

        formatter.WriteTable(
            new List<TestItem>(),
            ("Name", i => i.Name));

        console.Output.Should().Contain("No items found");
    }

    [Fact]
    public void WriteObject_JsonFormat_WritesValidJson()
    {
        var console = new TestConsole();
        var context = new CliContext { OutputFormat = OutputFormat.Json };
        var formatter = new OutputFormatter(context, console);
        var item = new TestItem { Name = "Gamma", Value = 42 };

        formatter.WriteObject(
            item,
            ("Name", "Gamma"),
            ("Value", "42"));

        var output = console.Output;
        output.Should().Contain("\"name\"");
        output.Should().Contain("\"Gamma\"");
        output.Should().Contain("42");
    }

    [Fact]
    public void WriteObject_TableFormat_WritesLabelValuePairs()
    {
        var console = new TestConsole();
        var context = new CliContext { OutputFormat = OutputFormat.Table };
        var formatter = new OutputFormatter(context, console);
        var item = new TestItem { Name = "Delta", Value = 7 };

        formatter.WriteObject(
            item,
            ("Name", "Delta"),
            ("Value", "7"));

        var output = console.Output;
        output.Should().Contain("Name");
        output.Should().Contain("Delta");
        output.Should().Contain("Value");
        output.Should().Contain("7");
    }

    [Fact]
    public void WriteValidationResults_ErrorsAndWarnings_FormatsCorrectly()
    {
        var console = new TestConsole();
        var context = new CliContext();
        var formatter = new OutputFormatter(context, console);

        var errors = new List<(string Code, string Message, string? FilePath)>
        {
            ("CFG-001", "Missing adapter reference", "devices/test.yaml")
        };
        var warnings = new List<(string Code, string Message, string? FilePath)>
        {
            ("CFG-W01", "Empty room", null)
        };

        formatter.WriteValidationResults(errors, warnings);

        var output = console.Output;
        output.Should().Contain("1 error(s)");
        output.Should().Contain("1 warning(s)");
        output.Should().Contain("[CFG-001]");
        output.Should().Contain("Missing adapter reference");
        output.Should().Contain("devices/test.yaml");
        output.Should().Contain("[CFG-W01]");
        output.Should().Contain("Empty room");
    }

    [Fact]
    public void WriteValidationResults_NoIssues_WritesNothing()
    {
        var console = new TestConsole();
        var context = new CliContext();
        var formatter = new OutputFormatter(context, console);

        formatter.WriteValidationResults([], []);

        console.Output.Should().BeEmpty();
    }

    [Fact]
    public void WriteSuccess_WritesGreenMessage()
    {
        var console = new TestConsole();
        var context = new CliContext();
        var formatter = new OutputFormatter(context, console);

        formatter.WriteSuccess("Done!");

        console.Output.Should().Contain("Done!");
    }

    [Fact]
    public void WriteError_WritesErrorMessage()
    {
        var console = new TestConsole();
        var context = new CliContext();
        var formatter = new OutputFormatter(context, console);

        formatter.WriteError("Something failed");

        console.Output.Should().Contain("Error: Something failed");
    }

    [Fact]
    public void WriteWarning_WritesWarningMessage()
    {
        var console = new TestConsole();
        var context = new CliContext();
        var formatter = new OutputFormatter(context, console);

        formatter.WriteWarning("Be careful");

        console.Output.Should().Contain("Warning: Be careful");
    }

    [Fact]
    public void WriteTable_TableFormat_NonEmptyList_RendersColumns()
    {
        // Arrange
        var console = new TestConsole();
        var context = new CliContext { OutputFormat = OutputFormat.Table };
        var formatter = new OutputFormatter(context, console);
        var items = new List<TestItem>
        {
            new TestItem { Name = "Alpha", Value = 1 },
            new TestItem { Name = "Beta", Value = 2 }
        };

        // Act
        formatter.WriteTable(
            items,
            ("Name", i => i.Name),
            ("Value", i => i.Value.ToString()));

        // Assert
        var output = console.Output;
        output.Should().Contain("Alpha");
        output.Should().Contain("Beta");
        output.Should().Contain("Name");
        output.Should().Contain("Value");
    }

    [Fact]
    public void WriteValidationResults_ErrorsWithFilePath_IncludesFilePath()
    {
        // Arrange
        var console = new TestConsole();
        var context = new CliContext();
        var formatter = new OutputFormatter(context, console);
        var errors = new List<(string Code, string Message, string? FilePath)>
        {
            ("E01", "Bad thing", "path/to/file.yaml")
        };

        // Act
        formatter.WriteValidationResults(errors, []);

        // Assert
        console.Output.Should().Contain("path/to/file.yaml");
    }

    [Fact]
    public void WriteValidationResults_ErrorsWithoutFilePath_OmitsFileLine()
    {
        // Arrange
        var console = new TestConsole();
        var context = new CliContext();
        var formatter = new OutputFormatter(context, console);
        var errors = new List<(string Code, string Message, string? FilePath)>
        {
            ("E01", "Bad thing", null)
        };

        // Act
        formatter.WriteValidationResults(errors, []);

        // Assert
        console.Output.Should().NotContain("File:");
    }

    [Fact]
    public void WriteObject_YamlFormat_WritesYaml()
    {
        // Arrange
        var console = new TestConsole();
        var context = new CliContext { OutputFormat = OutputFormat.Yaml };
        var formatter = new OutputFormatter(context, console);
        var item = new TestItem { Name = "Epsilon", Value = 99 };

        // Act
        formatter.WriteObject(
            item,
            ("Name", "Epsilon"),
            ("Value", "99"));

        // Assert
        var output = console.Output;
        output.Should().Contain("Epsilon");
        output.Should().Contain("99");
    }

    private class TestItem
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
