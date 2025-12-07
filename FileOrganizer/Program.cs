using Microsoft.Extensions.Configuration;
using System.CommandLine;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

var sourceFolder = configuration["SourceFolder"] ?? throw new InvalidOperationException("SourceFolder not configured");
var outputFileName = ExpandDateTimeMacros(configuration["OutputFileName"] ?? throw new InvalidOperationException("OutputFileName not configured"));
var fileExtensions = configuration["FileExtensions"] ?? throw new InvalidOperationException("FileExtensions not configured");
var targetFolderPattern = configuration["TargetFolderPattern"] ?? throw new InvalidOperationException("TargetFolderPattern not configured");
var copyLogFileName = ExpandDateTimeMacros(configuration["CopyLogFileName"] ?? "copy-log.txt");

var analyzeOption = new Option<bool>(
    name: "--analyze",
    description: "Analyze files in the source folder and generate report");

var copyOption = new Option<bool>(
    name: "--copy",
    description: "Copy files with specified extensions to target folder with date-based structure");

var rootCommand = new RootCommand("File Organizer - analyze files by extension");
rootCommand.AddOption(analyzeOption);
rootCommand.AddOption(copyOption);

rootCommand.SetHandler((analyze, copy) =>
{
    if (analyze)
    {
        AnalyzeFiles(sourceFolder, outputFileName);
    }
    else if (copy)
    {
        CopyFiles(sourceFolder, fileExtensions, targetFolderPattern, copyLogFileName);
    }
    else
    {
        Console.WriteLine("Use --analyze to analyze files in the configured folder");
        Console.WriteLine("Use --copy to copy files with specified extensions to target folder");
    }
}, analyzeOption, copyOption);

return await rootCommand.InvokeAsync(args);

static void AnalyzeFiles(string sourceFolder, string outputFileName)
{
    Console.WriteLine($"Analyzing folder: {sourceFolder}");
    
    if (!Directory.Exists(sourceFolder))
    {
        Console.WriteLine($"Error: Source folder '{sourceFolder}' does not exist!");
        return;
    }

    var files = Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories);
    var extensionCounts = CountFilesByExtension(files);
    
    SaveAnalysisReport(outputFileName, sourceFolder, files, extensionCounts);
    
    Console.WriteLine($"Analysis complete! Results saved to: {outputFileName}");
    Console.WriteLine($"Total files analyzed: {files.Length}");
    Console.WriteLine($"Unique extensions: {extensionCounts.Count}");
}

static Dictionary<string, int> CountFilesByExtension(string[] files)
{
    var extensionCounts = new Dictionary<string, int>();
    var processedCount = 0;
    var lastReportTime = DateTime.Now;
    var totalFiles = files.Length;
    
    Console.WriteLine($"Found {totalFiles} files to analyze...");
    
    foreach (var file in files)
    {
        var extension = Path.GetExtension(file).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension))
            extension = "(no extension)";
        
        if (extensionCounts.ContainsKey(extension))
            extensionCounts[extension]++;
        else
            extensionCounts[extension] = 1;
        
        processedCount++;
        
        if ((DateTime.Now - lastReportTime).TotalSeconds >= 1)
        {
            Console.WriteLine($"Processed: {processedCount}/{totalFiles} files ({processedCount * 100.0 / totalFiles:F1}%)");
            lastReportTime = DateTime.Now;
        }
    }
    
    Console.WriteLine($"Processed: {processedCount}/{totalFiles} files (100.0%)");
    
    return extensionCounts;
}

static void SaveAnalysisReport(string outputFileName, string sourceFolder, string[] files, Dictionary<string, int> extensionCounts)
{
    var sortedResults = extensionCounts
        .OrderByDescending(x => x.Value)
        .ThenBy(x => x.Key);
    
    using var writer = new StreamWriter(outputFileName);
    writer.WriteLine($"File Analysis Report - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    writer.WriteLine($"Source Folder: {sourceFolder}");
    writer.WriteLine($"Total Files: {files.Length}");
    writer.WriteLine(new string('-', 50));
    writer.WriteLine();
    
    foreach (var result in sortedResults)
    {
        writer.WriteLine($"{result.Key,-20} {result.Value,10} files");
    }
}

static void CopyFiles(string sourceFolder, string fileExtensions, string targetFolderPattern, string copyLogFileName)
{
    Console.WriteLine($"Copying files from: {sourceFolder}");
    
    if (!Directory.Exists(sourceFolder))
    {
        Console.WriteLine($"Error: Source folder '{sourceFolder}' does not exist!");
        return;
    }

    var allowedExtensions = ParseAllowedExtensions(fileExtensions);
    Console.WriteLine($"Allowed extensions: {string.Join(", ", allowedExtensions)}");

    var filesToCopy = GetFilesToCopy(sourceFolder, allowedExtensions);
    
    using var logWriter = new StreamWriter(copyLogFileName, append: false);
    logWriter.WriteLine($"File Copy Operation Log - Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    logWriter.WriteLine(new string('=', 100));
    logWriter.WriteLine();
    
    var (copiedCount, skippedCount) = ProcessFileCopying(filesToCopy, targetFolderPattern, logWriter);

    logWriter.WriteLine();
    logWriter.WriteLine(new string('=', 100));
    logWriter.WriteLine($"Operation completed: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    logWriter.WriteLine($"Total files processed: {filesToCopy.Length}");
    logWriter.WriteLine($"Files copied: {copiedCount}");
    logWriter.WriteLine($"Files skipped: {skippedCount}");

    Console.WriteLine($"Copy complete! Files copied: {copiedCount}, Skipped: {skippedCount}");
    Console.WriteLine($"Detailed log saved to: {copyLogFileName}");
}

static HashSet<string> ParseAllowedExtensions(string fileExtensions)
{
    return fileExtensions
        .Split(';', StringSplitOptions.RemoveEmptyEntries)
        .Select(ext => ext.Trim().ToLowerInvariant())
        .ToHashSet();
}

static string[] GetFilesToCopy(string sourceFolder, HashSet<string> allowedExtensions)
{
    var allFiles = Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories);
    return allFiles.Where(f => allowedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant())).ToArray();
}

static (int copiedCount, int skippedCount) ProcessFileCopying(string[] filesToCopy, string targetFolderPattern, StreamWriter logWriter)
{
    var totalFiles = filesToCopy.Length;
    var processedCount = 0;
    var copiedCount = 0;
    var skippedCount = 0;
    var lastReportTime = DateTime.Now;

    Console.WriteLine($"Found {totalFiles} files to copy...");

    foreach (var sourceFile in filesToCopy)
    {
        processedCount++;
        var operation = CopySingleFile(sourceFile, targetFolderPattern, logWriter, processedCount, totalFiles);
        
        if (operation?.Result == CopyResult.Copied)
            copiedCount++;
        else
            skippedCount++;

        if ((DateTime.Now - lastReportTime).TotalSeconds >= 1)
        {
            Console.WriteLine($"Processed: {processedCount}/{totalFiles} files ({processedCount * 100.0 / totalFiles:F1}%) - Copied: {copiedCount}, Skipped: {skippedCount}");
            lastReportTime = DateTime.Now;
        }
    }

    Console.WriteLine($"Processed: {processedCount}/{totalFiles} files (100.0%) - Copied: {copiedCount}, Skipped: {skippedCount}");

    return (copiedCount, skippedCount);
}

static CopyOperation? CopySingleFile(string sourceFile, string targetFolderPattern, StreamWriter logWriter, int fileIndex, int totalFiles)
{
    try
    {
        var fileInfo = new FileInfo(sourceFile);
        var targetFullPath = BuildTargetPath(sourceFile, fileInfo, targetFolderPattern);
        var originalTargetPath = targetFullPath;
        
        var (finalTargetPath, collisionResult) = ResolveTargetPathWithCollisionHandling(sourceFile, targetFullPath);
        
        EnsureDirectoryExists(finalTargetPath);
        
        var operation = CopyFileIfNeeded(sourceFile, finalTargetPath, fileInfo, collisionResult, originalTargetPath);
        LogCopyOperation(operation, logWriter, fileIndex, totalFiles);
        
        return operation;
    }
    catch (Exception ex)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        logWriter.WriteLine($"[{timestamp}] [{fileIndex}/{totalFiles}] ERROR: Failed to copy '{sourceFile}' - {ex.Message}");
        Console.WriteLine($"Error copying file '{sourceFile}': {ex.Message}");
        return null;
    }
}

static (string targetPath, CopyResult? collisionResult) ResolveTargetPathWithCollisionHandling(string sourceFile, string targetFullPath)
{
    if (!File.Exists(targetFullPath))
    {
        return (targetFullPath, null);
    }

    var sourceInfo = new FileInfo(sourceFile);
    var targetInfo = new FileInfo(targetFullPath);
    
    if (sourceInfo.Length != targetInfo.Length)
    {
        var uniquePath = GetUniqueTargetPath(sourceFile, targetFullPath, out var skipResult);
        return (uniquePath, skipResult);
    }

    if (AreFilesIdenticalByHash(sourceFile, targetFullPath))
    {
        return (targetFullPath, CopyResult.SkippedSameHash);
    }

    var newPath = GetUniqueTargetPath(sourceFile, targetFullPath, out var result);
    return (newPath, result);
}

static string GetUniqueTargetPath(string sourceFile, string originalTargetPath, out CopyResult? skipResult)
{
    skipResult = CopyResult.SkippedDifferentHash;
    
    var directory = Path.GetDirectoryName(originalTargetPath);
    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalTargetPath);
    var extension = Path.GetExtension(originalTargetPath);

    var counter = 1;
    string newTargetPath;

    do
    {
        var newFileName = $"{fileNameWithoutExt}_{counter}{extension}";
        newTargetPath = Path.Combine(directory!, newFileName);

        if (!File.Exists(newTargetPath))
        {
            skipResult = null;
            return newTargetPath;
        }

        var sourceInfo = new FileInfo(sourceFile);
        var targetInfo = new FileInfo(newTargetPath);
        
        if (sourceInfo.Length == targetInfo.Length && AreFilesIdenticalByHash(sourceFile, newTargetPath))
        {
            skipResult = CopyResult.SkippedSameHash;
            return newTargetPath;
        }

        counter++;
    } while (counter < 10000);

    throw new InvalidOperationException($"Too many file collisions for: {originalTargetPath}");
}

static bool AreFilesIdenticalByHash(string file1, string file2)
{
    var hash1 = ComputeFileHash(file1);
    var hash2 = ComputeFileHash(file2);
    return hash1.SequenceEqual(hash2);
}

static byte[] ComputeFileHash(string filePath)
{
    using var stream = File.OpenRead(filePath);
    using var sha256 = SHA256.Create();
    return sha256.ComputeHash(stream);
}

static CopyOperation CopyFileIfNeeded(string sourceFile, string targetFullPath, FileInfo sourceFileInfo, CopyResult? collisionResult, string originalTargetPath)
{
    var fileSize = sourceFileInfo.Length;
    
    if (collisionResult == CopyResult.SkippedSameHash)
    {
        return new CopyOperation(CopyResult.SkippedSameHash, sourceFile, targetFullPath, fileSize, originalTargetPath);
    }
    
    if (collisionResult == CopyResult.SkippedDifferentHash)
    {
        File.Copy(sourceFile, targetFullPath, overwrite: false);
        File.SetLastWriteTime(targetFullPath, sourceFileInfo.LastWriteTime);
        return new CopyOperation(CopyResult.Copied, sourceFile, targetFullPath, fileSize, originalTargetPath);
    }
    
    if (!File.Exists(targetFullPath))
    {
        File.Copy(sourceFile, targetFullPath, overwrite: false);
        File.SetLastWriteTime(targetFullPath, sourceFileInfo.LastWriteTime);
        return new CopyOperation(CopyResult.Copied, sourceFile, targetFullPath, fileSize);
    }
    
    return new CopyOperation(CopyResult.SkippedSameHash, sourceFile, targetFullPath, fileSize);
}

static void LogCopyOperation(CopyOperation? operation, StreamWriter logWriter, int fileIndex, int totalFiles)
{
    if (operation == null) return;
    
    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    var sizeKB = operation.FileSize / 1024.0;
    var sizeMB = sizeKB / 1024.0;
    var sizeStr = sizeMB >= 1 ? $"{sizeMB:F2} MB" : $"{sizeKB:F2} KB";
    
    switch (operation.Result)
    {
        case CopyResult.Copied:
            if (operation.OriginalTargetPath != null && operation.OriginalTargetPath != operation.TargetPath)
            {
                logWriter.WriteLine($"[{timestamp}] [{fileIndex}/{totalFiles}] COPY (DUPLICATE - DIFFERENT HASH)");
                logWriter.WriteLine($"  Source: {operation.SourcePath} ({sizeStr})");
                logWriter.WriteLine($"  Original Target: {operation.OriginalTargetPath}");
                logWriter.WriteLine($"  Actual Target: {operation.TargetPath}");
            }
            else
            {
                logWriter.WriteLine($"[{timestamp}] [{fileIndex}/{totalFiles}] COPY");
                logWriter.WriteLine($"  Source: {operation.SourcePath} ({sizeStr})");
                logWriter.WriteLine($"  Target: {operation.TargetPath}");
            }
            break;
            
        case CopyResult.SkippedSameHash:
            logWriter.WriteLine($"[{timestamp}] [{fileIndex}/{totalFiles}] SKIP (DUPLICATE - SAME HASH)");
            logWriter.WriteLine($"  Source: {operation.SourcePath} ({sizeStr})");
            logWriter.WriteLine($"  Target: {operation.TargetPath}");
            break;
    }
    
    logWriter.WriteLine();
}

static string BuildTargetPath(string sourceFile, FileInfo fileInfo, string targetFolderPattern)
{
    var fileDate = fileInfo.LastWriteTime;
    var fileName = Path.GetFileNameWithoutExtension(sourceFile);
    var fileExtension = Path.GetExtension(sourceFile).TrimStart('.');

    var targetPath = targetFolderPattern
        .Replace("{FileDate:yyyy}", fileDate.ToString("yyyy"))
        .Replace("{FileDate:MM}", fileDate.ToString("MM"))
        .Replace("{FileDate:yyyyMMdd_HHmmss}", fileDate.ToString("yyyyMMdd_HHmmss"))
        .Replace("{FileName}", fileName)
        .Replace("{FileExtension}", fileExtension);

    return targetPath;
}

static void EnsureDirectoryExists(string targetFullPath)
{
    var targetDirectory = Path.GetDirectoryName(targetFullPath);
    if (!string.IsNullOrEmpty(targetDirectory) && !Directory.Exists(targetDirectory))
    {
        Directory.CreateDirectory(targetDirectory);
    }
}

static string ExpandDateTimeMacros(string path)
{
    var now = DateTime.Now;
    
    // Regex pattern to match {DateTime:format}
    var pattern = @"\{DateTime:([^}]+)\}";
    
    return Regex.Replace(path, pattern, match =>
    {
        var format = match.Groups[1].Value;
        return now.ToString(format);
    });
}

enum CopyResult
{
    Copied,
    SkippedSameHash,
    SkippedDifferentHash
}

record CopyOperation(CopyResult Result, string SourcePath, string TargetPath, long FileSize, string? OriginalTargetPath = null);
