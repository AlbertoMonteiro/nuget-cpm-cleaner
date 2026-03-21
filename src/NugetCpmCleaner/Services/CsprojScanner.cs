namespace NugetCpmCleaner.Services;

using System.Diagnostics;
using System.Text.Json;
using Spectre.Console;

public sealed class CsprojScanner
{
    public HashSet<string> ScanUsedPackages(string rootDirectory)
    {
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var projects = Directory.EnumerateFiles(rootDirectory, "*.csproj", SearchOption.AllDirectories).ToList();

        AnsiConsole.Progress()
            .AutoClear(true)
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new CounterColumn(projects.Count),
                new SpinnerColumn())
            .Start(ctx =>
            {
                var task = ctx.AddTask("Evaluating projects", maxValue: projects.Count);

                foreach (var csproj in projects)
                {
                    task.Description = $"[grey]{Markup.Escape(Path.GetFileName(csproj))}[/]";

                    try
                    {
                        var output = RunMsBuild(csproj);
                        foreach (var name in ParsePackageNames(output))
                            used.Add(name);
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"[yellow]Warning:[/] Could not evaluate [grey]{Markup.Escape(csproj)}[/]: {Markup.Escape(ex.Message)}");
                    }

                    task.Increment(1);
                }
            });

        return used;
    }

    private static string RunMsBuild(string csprojPath)
    {
        var psi = new ProcessStartInfo("dotnet", $"msbuild \"{csprojPath}\" -getItem:PackageReference")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(psi)!;
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return output;
    }

    private static IEnumerable<string> ParsePackageNames(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            yield break;

        using var doc = JsonDocument.Parse(json);

        // dotnet msbuild -getItem returns: { "Items": { "PackageReference": [...] } }
        // or directly: { "PackageReference": [...] } depending on the SDK version
        var root = doc.RootElement;
        if (root.TryGetProperty("Items", out var items))
            root = items;

        if (!root.TryGetProperty("PackageReference", out var packages))
            yield break;

        foreach (var package in packages.EnumerateArray())
        {
            if (package.TryGetProperty("Identity", out var identity))
                yield return identity.GetString()!;
        }
    }
}
