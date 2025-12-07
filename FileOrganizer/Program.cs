using Microsoft.Extensions.Configuration;
using System.CommandLine;
using FileOrganizer;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

var appConfig = new AppConfiguration(configuration);

var analyzeOption = new Option<bool>(
    name: "--analyze",
    description: "Analyze files in the source folder and generate report");

var copyOption = new Option<bool>(
    name: "--copy",
    description: "Copy files with specified extensions to target folder with date-based structure");

var dryRunOption = new Option<bool>(
    name: "--dry-run",
    description: "Simulate the copy operation without actually copying files (preview mode)");

var rootCommand = new RootCommand("File Organizer - analyze files by extension");
rootCommand.AddOption(analyzeOption);
rootCommand.AddOption(copyOption);
rootCommand.AddOption(dryRunOption);

rootCommand.SetHandler((analyze, copy, dryRun) =>
{
    if (analyze)
    {
        var analyzer = new FileAnalyzer();
        analyzer.AnalyzeFiles(appConfig.SourceFolder, appConfig.OutputFileName);
    }
    else if (copy)
    {
        var copier = new FileCopier();
        copier.CopyFiles(appConfig.SourceFolder, appConfig.FileExtensions, appConfig.TargetFolderPattern, appConfig.CopyLogFileName, dryRun);
    }
    else
    {
        Console.WriteLine("Use --analyze to analyze files in the configured folder");
        Console.WriteLine("Use --copy to copy files with specified extensions to target folder");
        Console.WriteLine("Add --dry-run to preview copy operations without actually copying files");
    }
}, analyzeOption, copyOption, dryRunOption);

return await rootCommand.InvokeAsync(args);
