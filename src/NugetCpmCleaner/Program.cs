using System.Text;
using NugetCpmCleaner.Commands;
using Spectre.Console.Cli;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

var app = new CommandApp<CleanCommand>();

app.Configure(config =>
{
    config.SetApplicationName("nuget-cpm-cleaner");
    config.SetApplicationVersion("1.0.0");
    config.AddExample(["--root", "C:/repos/my-solution"]);
    config.AddExample(["--root", ".", "--auto-remove"]);
});

return app.Run(args);
