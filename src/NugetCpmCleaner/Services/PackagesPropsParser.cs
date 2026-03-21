namespace NugetCpmCleaner.Services;

using System.Xml.Linq;
using NugetCpmCleaner.Models;

public sealed class PackagesPropsParser
{
    public (XDocument Document, IReadOnlyList<PackageEntry> Entries) Parse(string filePath)
    {
        var doc = XDocument.Load(filePath, LoadOptions.PreserveWhitespace);

        var entries = doc
            .Descendants("PackageVersion")
            .Select(e => e.Attribute("Include")?.Value)
            .Where(name => name is not null)
            .Select(name => new PackageEntry(name!))
            .ToList();

        return (doc, entries);
    }
}
