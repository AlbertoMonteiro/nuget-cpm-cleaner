namespace NugetCpmCleaner.Commands;

using System.ComponentModel;
using Spectre.Console.Cli;

public sealed class CleanCommandSettings : CommandSettings
{
    [Description("Path to the repository root. Defaults to the current directory.")]
    [CommandOption("--root")]
    public string Root { get; init; } = Directory.GetCurrentDirectory();

    [Description("Automatically remove all unused packages without prompting.")]
    [CommandOption("--auto-remove")]
    [DefaultValue(false)]
    public bool AutoRemove { get; init; }
}
