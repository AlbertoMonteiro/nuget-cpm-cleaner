namespace NugetCpmCleaner.Services;

using System.Xml;
using System.Xml.Linq;
using Spectre.Console;

public sealed class CsprojScanner
{
    public HashSet<string> ScanUsedPackages(string rootDirectory)
    {
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var files = Directory
            .EnumerateFiles(rootDirectory, "*.csproj", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(rootDirectory, "Directory.Build.props", SearchOption.AllDirectories));

        foreach (var file in files)
        {
            try
            {
                var doc = XDocument.Load(file);
                foreach (var name in doc
                    .Descendants("PackageReference")
                    .Select(e => e.Attribute("Include")?.Value)
                    .Where(n => n is not null))
                {
                    used.Add(name!);
                }
            }
            catch (XmlException ex)
            {
                AnsiConsole.MarkupLine($"[yellow]Warning:[/] Could not parse [grey]{Markup.Escape(file)}[/]: {Markup.Escape(ex.Message)}");
            }
        }

        return used;
    }
}
