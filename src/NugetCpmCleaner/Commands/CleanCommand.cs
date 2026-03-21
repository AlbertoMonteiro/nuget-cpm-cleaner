namespace NugetCpmCleaner.Commands;

using Spectre.Console;
using Spectre.Console.Cli;
using NugetCpmCleaner.Services;

public sealed class CleanCommand : Command<CleanCommandSettings>
{
    private readonly PackagesPropsParser _parser = new();
    private readonly CsprojScanner _scanner = new();
    private readonly PackagesPropsWriter _writer = new();

    public override int Execute(CommandContext context, CleanCommandSettings settings)
    {
        var root = Path.GetFullPath(settings.Root);

        // 1. Locate Directory.Packages.props
        var propsFile = FindPackagesPropsFile(root);
        if (propsFile is null)
        {
            AnsiConsole.MarkupLine("[red]Error:[/] Could not find [bold]Directory.Packages.props[/] at or above the specified root.");
            return 1;
        }

        AnsiConsole.MarkupLine($"Found [bold]{Markup.Escape(propsFile)}[/]");

        // 2. Parse declared packages
        var (document, declared) = _parser.Parse(propsFile);
        AnsiConsole.MarkupLine($"Declared packages: [bold]{declared.Count}[/]");

        // 3. Scan .csproj files
        var used = _scanner.ScanUsedPackages(root);
        AnsiConsole.MarkupLine($"Referenced packages across .csproj files: [bold]{used.Count}[/]");

        // 4. Compute unused
        var unused = declared
            .Where(p => !used.Contains(p.Name))
            .OrderBy(p => p.Name)
            .ToList();

        if (unused.Count == 0)
        {
            AnsiConsole.MarkupLine("\n[green]All declared packages are in use. Nothing to remove.[/]");
            return 0;
        }

        AnsiConsole.MarkupLine($"\nFound [yellow]{unused.Count}[/] unused package(s):");

        // 5. Select which to remove
        List<string> toRemove;

        if (settings.AutoRemove)
        {
            toRemove = unused.Select(p => p.Name).ToList();
            AnsiConsole.MarkupLine("");
            foreach (var name in toRemove)
                AnsiConsole.MarkupLine($"  [grey]·[/] {Markup.Escape(name)}");
        }
        else
        {
            toRemove = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title("\nSelect packages to [red]remove[/]:")
                    .NotRequired()
                    .PageSize(20)
                    .MoreChoicesText("[grey](Move up and down to reveal more packages)[/]")
                    .InstructionsText("[grey](Press [blue]<space>[/] to toggle, [green]<enter>[/] to confirm)[/]")
                    .AddChoices(unused.Select(p => p.Name)));
        }

        if (toRemove.Count == 0)
        {
            AnsiConsole.MarkupLine("\n[grey]No packages selected. Nothing was changed.[/]");
            return 0;
        }

        // 6. Apply removal
        _writer.RemovePackages(document, propsFile, toRemove);

        // 7. Summary
        AnsiConsole.MarkupLine($"\n[green]Done.[/] Removed [bold]{toRemove.Count}[/] package(s) from [bold]{Path.GetFileName(propsFile)}[/]:");
        foreach (var name in toRemove)
            AnsiConsole.MarkupLine($"  [red]-[/] {Markup.Escape(name)}");

        return 0;
    }

    private static string? FindPackagesPropsFile(string startDirectory)
    {
        var dir = new DirectoryInfo(startDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "Directory.Packages.props");
            if (File.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }
        return null;
    }
}
