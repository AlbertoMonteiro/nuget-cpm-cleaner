using NugetCpmCleaner.Commands;
using Spectre.Console.Cli;

var app = new CommandApp<CleanCommand>();

app.Configure(config =>
{
    config.SetApplicationName("nuget-cpm-cleaner");
    config.SetApplicationVersion("1.0.0");
    config.AddExample(["--root", "C:/repos/my-solution"]);
    config.AddExample(["--root", ".", "--auto-remove"]);
});

return app.Run(args);
