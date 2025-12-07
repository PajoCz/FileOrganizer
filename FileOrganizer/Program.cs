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
    description: "Simulate the operation without actually copying/deleting files (preview mode)");

var findDuplicatesOption = new Option<bool>(
    name: "--find-duplicates",
    description: "Find duplicate files in target folder (same content, different timestamps in filename)");

var cleanDuplicatesOption = new Option<bool>(
    name: "--clean-duplicates",
    description: "Clean duplicates based on analysis from --find-duplicates");

var duplicatesLogOption = new Option<string?>(
    name: "--duplicates-log",
    description: "Path to specific duplicates log file (auto-detects latest if not specified)");

var rootCommand = new RootCommand("File Organizer - analyze and organize files by date");
rootCommand.AddOption(analyzeOption);
rootCommand.AddOption(copyOption);
rootCommand.AddOption(dryRunOption);
rootCommand.AddOption(findDuplicatesOption);
rootCommand.AddOption(cleanDuplicatesOption);
rootCommand.AddOption(duplicatesLogOption);

rootCommand.SetHandler((analyze, copy, dryRun, findDuplicates, cleanDuplicates, duplicatesLog) =>
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
    else if (findDuplicates)
    {
        var targetFolder = Path.GetDirectoryName(appConfig.TargetFolderPattern) 
            ?? throw new InvalidOperationException("Cannot determine target folder from TargetFolderPattern");
        
        // Extract base path (remove {FileDate:...} macros)
        var basePath = System.Text.RegularExpressions.Regex.Replace(targetFolder, @"\{[^}]+\}", "");
        basePath = basePath.TrimEnd('\\', '/');
        
        var finder = new DuplicateFinder(appConfig.FileNamePrefix);
        var logFileName = appConfig.DuplicatesLogFileName;
        finder.FindDuplicates(basePath, logFileName);
        
        Console.WriteLine();
        Console.WriteLine($"To clean these duplicates, run:");
        Console.WriteLine($"  dotnet run --clean-duplicates --duplicates-log \"{logFileName}\"");
        Console.WriteLine($"Or to preview:");
        Console.WriteLine($"  dotnet run --clean-duplicates --duplicates-log \"{logFileName}\" --dry-run");
    }
    else if (cleanDuplicates)
    {
        var cleaner = new DuplicateCleaner();
        
        // Use specified log file or find the latest
        var logFileToUse = duplicatesLog;
        
        if (string.IsNullOrEmpty(logFileToUse))
        {
            // Auto-detect latest duplicates log file
            var logPattern = "duplicates-log*.txt";
            var logFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), logPattern)
                                    .OrderByDescending(f => new FileInfo(f).LastWriteTime)
                                    .ToArray();
            
            if (logFiles.Length == 0)
            {
                Console.WriteLine($"Error: No duplicates log files found matching pattern '{logPattern}'");
                Console.WriteLine("Run --find-duplicates first to generate a log file.");
                Console.WriteLine("Or specify a log file with --duplicates-log <path>");
                return;
            }
            
            logFileToUse = logFiles[0];
            Console.WriteLine($"Auto-detected duplicates log: {Path.GetFileName(logFileToUse)}");
            Console.WriteLine();
        }
        
        cleaner.CleanDuplicates(logFileToUse, appConfig.CleanupLogFileName, dryRun);
    }
    else
    {
        Console.WriteLine("File Organizer - Available commands:");
        Console.WriteLine();
        Console.WriteLine("  --analyze            Analyze files in source folder");
        Console.WriteLine("  --copy               Copy and organize files to target folder");
        Console.WriteLine("  --find-duplicates    Find duplicate files in target folder");
        Console.WriteLine("  --clean-duplicates   Clean duplicates (keep files with matching dates)");
        Console.WriteLine();
        Console.WriteLine("Optional flags:");
        Console.WriteLine("  --dry-run            Preview mode (no actual changes)");
        Console.WriteLine("  --duplicates-log <path>  Specify duplicates log file (auto-detects latest if not specified)");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  dotnet run --analyze");
        Console.WriteLine("  dotnet run --copy --dry-run");
        Console.WriteLine("  dotnet run --find-duplicates");
        Console.WriteLine("  dotnet run --clean-duplicates");
        Console.WriteLine("  dotnet run --clean-duplicates --dry-run");
        Console.WriteLine("  dotnet run --clean-duplicates --duplicates-log \"duplicates-log_20241207_130604.txt\"");
    }
}, analyzeOption, copyOption, dryRunOption, findDuplicatesOption, cleanDuplicatesOption, duplicatesLogOption);

return await rootCommand.InvokeAsync(args);
