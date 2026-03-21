namespace NugetCpmCleaner.Services;

using System.Xml.Linq;

public sealed class PackagesPropsWriter
{
    public void RemovePackages(XDocument document, string filePath, IEnumerable<string> packageNamesToRemove)
    {
        var namesToRemove = new HashSet<string>(packageNamesToRemove, StringComparer.OrdinalIgnoreCase);

        var nodesToRemove = document
            .Descendants("PackageVersion")
            .Where(e => namesToRemove.Contains(e.Attribute("Include")?.Value ?? ""))
            .ToList();

        foreach (var node in nodesToRemove)
        {
            // Remove preceding whitespace text node (indentation) to avoid leaving blank lines
            if (node.PreviousNode is XText prev && string.IsNullOrWhiteSpace(prev.Value))
                prev.Remove();

            node.Remove();
        }

        document.Save(filePath, SaveOptions.DisableFormatting);
    }
}
