namespace NugetCpmCleaner;

using Spectre.Console;
using Spectre.Console.Rendering;

public sealed class CounterColumn(int total) : ProgressColumn
{
    public override IRenderable Render(RenderOptions options, ProgressTask task, TimeSpan deltaTime)
        => new Markup($"[grey]{(int)task.Value}/{total}[/]");
}
