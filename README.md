# nuget-cpm-cleaner

A .NET global tool that removes unused `<PackageVersion>` entries from your `Directory.Packages.props` file ([NuGet Central Package Management](https://learn.microsoft.com/nuget/consume-packages/central-package-management)).

## The Problem

When using NuGet CPM, over time packages get removed from `.csproj` files but their version declarations stay behind in `Directory.Packages.props`, causing noise and potential confusion.

## How It Works

1. Locates `Directory.Packages.props` starting from the specified root (walks up directories if needed)
2. Parses all `<PackageVersion>` entries — the "declared" packages
3. Recursively scans all `.csproj` and `Directory.Build.props` files for `<PackageReference>` entries — the "used" packages
4. Computes the difference: declared − used = **unused**
5. Either prompts you to select which ones to remove, or removes all automatically

## Installation

```bash
dotnet tool install -g nuget-cpm-cleaner
```

## Usage

### Interactive mode (default)

Displays a multi-select prompt listing all unused packages. Use `space` to toggle, `enter` to confirm.

```bash
nuget-cpm-cleaner --root C:/repos/my-solution
```

### Auto-remove mode

Removes all unused packages without prompting.

```bash
nuget-cpm-cleaner --root . --auto-remove
```

### Options

| Option | Description | Default |
|---|---|---|
| `--root` | Path to the repository root | Current directory |
| `--auto-remove` | Remove all unused packages without prompting | `false` |

## Example Output

```
Found C:\repos\my-solution\Directory.Packages.props
Declared packages: 42
Referenced packages across .csproj files: 38

Found 4 unused package(s):

? Select packages to remove: (Press <space> to toggle, <enter> to confirm)
> [ ] Deprecated.Package
  [ ] OldLibrary.Core
  [ ] SomeUnused.Tool
  [ ] UnusedAnalyzer

Done. Removed 2 package(s) from Directory.Packages.props:
  - Deprecated.Package
  - OldLibrary.Core
```

## Requirements

- .NET 8 or later

## License

MIT
