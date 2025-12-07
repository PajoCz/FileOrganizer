namespace FileOrganizer;

public class FileCopier
{
    private bool _dryRun;
    private DateTime _startTime;
    private long _totalBytesCopied;
    private long _totalBytesSkipped;
    
    public void CopyFiles(string sourceFolder, string fileExtensions, string targetFolderPattern, string copyLogFileName, bool dryRun = false)
    {
        _dryRun = dryRun;
        _startTime = DateTime.Now;
        _totalBytesCopied = 0;
        _totalBytesSkipped = 0;
        
        if (_dryRun)
        {
            Console.WriteLine("=== DRY RUN MODE - No files will be actually copied ===");
            Console.WriteLine();
        }
        
        Console.WriteLine($"Copying files from: {sourceFolder}");
        
        if (!Directory.Exists(sourceFolder))
        {
            Console.WriteLine($"Error: Source folder '{sourceFolder}' does not exist!");
            return;
        }

        var allowedExtensions = PathHelper.ParseFileExtensions(fileExtensions);
        Console.WriteLine($"Allowed extensions: {string.Join(", ", allowedExtensions)}");

        var filesToCopy = GetFilesToCopy(sourceFolder, allowedExtensions);
        
        using var logWriter = new StreamWriter(copyLogFileName, append: false);
        logWriter.WriteLine($"File Copy Operation Log{(_dryRun ? " (DRY RUN)" : "")} - Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        logWriter.WriteLine(new string('=', 100));
        if (_dryRun)
        {
            logWriter.WriteLine("DRY RUN MODE - No files were actually copied, this is a preview only");
            logWriter.WriteLine(new string('=', 100));
        }
        logWriter.WriteLine();
        
        var (copiedCount, skippedCount, errorCount) = ProcessFileCopying(filesToCopy, targetFolderPattern, logWriter);

        var endTime = DateTime.Now;
        var duration = endTime - _startTime;

        logWriter.WriteLine();
        logWriter.WriteLine(new string('=', 100));
        logWriter.WriteLine($"Operation completed: {endTime:yyyy-MM-dd HH:mm:ss}");
        logWriter.WriteLine($"Total files processed: {filesToCopy.Length}");
        logWriter.WriteLine($"Files {(_dryRun ? "would be " : "")}copied: {copiedCount}");
        logWriter.WriteLine($"Files {(_dryRun ? "would be " : "")}skipped: {skippedCount}");
        logWriter.WriteLine($"Errors: {errorCount}");
        logWriter.WriteLine();
        WriteStatistics(logWriter, copiedCount, skippedCount, errorCount, filesToCopy.Length, duration);
        
        if (_dryRun)
        {
            logWriter.WriteLine();
            logWriter.WriteLine("NOTE: This was a DRY RUN. No files were actually copied.");
        }

        Console.WriteLine();
        if (_dryRun)
        {
            Console.WriteLine("=== DRY RUN COMPLETED ===");
            Console.WriteLine($"Preview: {copiedCount} files would be copied, {skippedCount} would be skipped");
            Console.WriteLine("No files were actually copied. Run without --dry-run to perform the actual copy.");
        }
        else
        {
            Console.WriteLine($"Copy complete! Files copied: {copiedCount}, Skipped: {skippedCount}, Errors: {errorCount}");
        }
        
        Console.WriteLine();
        PrintStatistics(copiedCount, skippedCount, errorCount, filesToCopy.Length, duration);
        Console.WriteLine($"\nDetailed log saved to: {copyLogFileName}");
    }

    private void WriteStatistics(StreamWriter logWriter, int copiedCount, int skippedCount, int errorCount, int totalFiles, TimeSpan duration)
    {
        logWriter.WriteLine("=== STATISTICS ===");
        logWriter.WriteLine($"Total files processed: {totalFiles:N0}");
        logWriter.WriteLine($"Files copied: {copiedCount:N0} ({(totalFiles > 0 ? copiedCount * 100.0 / totalFiles : 0):F1}%)");
        logWriter.WriteLine($"Files skipped (duplicates): {skippedCount:N0} ({(totalFiles > 0 ? skippedCount * 100.0 / totalFiles : 0):F1}%)");
        logWriter.WriteLine($"Errors: {errorCount:N0} ({(totalFiles > 0 ? errorCount * 100.0 / totalFiles : 0):F1}%)");
        logWriter.WriteLine();
        
        logWriter.WriteLine($"Total data copied: {FormatBytes(_totalBytesCopied)}");
        logWriter.WriteLine($"Total data saved (duplicates): {FormatBytes(_totalBytesSkipped)}");
        logWriter.WriteLine($"Total data processed: {FormatBytes(_totalBytesCopied + _totalBytesSkipped)}");
        logWriter.WriteLine();
        
        logWriter.WriteLine($"Time elapsed: {FormatDuration(duration)}");
        
        if (duration.TotalSeconds > 0 && _totalBytesCopied > 0)
        {
            var bytesPerSecond = _totalBytesCopied / duration.TotalSeconds;
            logWriter.WriteLine($"Average copy speed: {FormatBytes((long)bytesPerSecond)}/s");
        }
        
        if (duration.TotalSeconds > 0 && totalFiles > 0)
        {
            var filesPerSecond = totalFiles / duration.TotalSeconds;
            logWriter.WriteLine($"Average processing speed: {filesPerSecond:F1} files/s");
        }
    }

    private void PrintStatistics(int copiedCount, int skippedCount, int errorCount, int totalFiles, TimeSpan duration)
    {
        Console.WriteLine("========================================================");
        Console.WriteLine("|                    STATISTICS                        |");
        Console.WriteLine("========================================================");
        Console.WriteLine($"| Total files processed:     {totalFiles,10:N0}           |");
        Console.WriteLine($"| Files copied:              {copiedCount,10:N0} ({copiedCount * 100.0 / totalFiles,5:F1}%) |");
        Console.WriteLine($"| Files skipped (duplicates):{skippedCount,10:N0} ({skippedCount * 100.0 / totalFiles,5:F1}%) |");
        Console.WriteLine($"| Errors:                    {errorCount,10:N0} ({errorCount * 100.0 / totalFiles,5:F1}%) |");
        Console.WriteLine("--------------------------------------------------------");
        Console.WriteLine($"| Total data copied:         {FormatBytes(_totalBytesCopied),20}    |");
        Console.WriteLine($"| Total data saved (dupl.):  {FormatBytes(_totalBytesSkipped),20}    |");
        Console.WriteLine($"| Total data processed:      {FormatBytes(_totalBytesCopied + _totalBytesSkipped),20}    |");
        Console.WriteLine("--------------------------------------------------------");
        Console.WriteLine($"| Time elapsed:              {FormatDuration(duration),20}    |");
        
        if (duration.TotalSeconds > 0 && _totalBytesCopied > 0)
        {
            var bytesPerSecond = _totalBytesCopied / duration.TotalSeconds;
            Console.WriteLine($"| Average copy speed:        {FormatBytes((long)bytesPerSecond) + "/s",20}    |");
        }
        
        if (duration.TotalSeconds > 0 && totalFiles > 0)
        {
            var filesPerSecond = totalFiles / duration.TotalSeconds;
            Console.WriteLine($"| Avg processing speed:      {filesPerSecond,15:F1} files/s |");
        }
        
        Console.WriteLine("========================================================");
    }

    private string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        
        return $"{len:F2} {sizes[order]}";
    }

    private string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalHours >= 1)
            return $"{(int)duration.TotalHours}h {duration.Minutes}m {duration.Seconds}s";
        else if (duration.TotalMinutes >= 1)
            return $"{duration.Minutes}m {duration.Seconds}s";
        else
            return $"{duration.Seconds}s";
    }

    private string[] GetFilesToCopy(string sourceFolder, HashSet<string> allowedExtensions)
    {
        var allFiles = Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories);
        return allFiles.Where(f => allowedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant())).ToArray();
    }

    private (int copiedCount, int skippedCount, int errorCount) ProcessFileCopying(string[] filesToCopy, string targetFolderPattern, StreamWriter logWriter)
    {
        var totalFiles = filesToCopy.Length;
        var processedCount = 0;
        var copiedCount = 0;
        var skippedCount = 0;
        var errorCount = 0;
        var lastReportTime = DateTime.Now;

        Console.WriteLine($"Found {totalFiles} files to {(_dryRun ? "preview" : "copy")}...");

        foreach (var sourceFile in filesToCopy)
        {
            processedCount++;
            var operation = CopySingleFile(sourceFile, targetFolderPattern, logWriter, processedCount, totalFiles);
            
            if (operation == null)
            {
                errorCount++;
            }
            else if (operation.Result == CopyResult.Copied)
            {
                copiedCount++;
                _totalBytesCopied += operation.FileSize;
            }
            else
            {
                skippedCount++;
                _totalBytesSkipped += operation.FileSize;
            }

            if ((DateTime.Now - lastReportTime).TotalSeconds >= 1)
            {
                var action = _dryRun ? "Previewed" : "Processed";
                Console.WriteLine($"{action}: {processedCount}/{totalFiles} files ({processedCount * 100.0 / totalFiles:F1}%) - {(_dryRun ? "Would copy" : "Copied")}: {copiedCount}, {(_dryRun ? "Would skip" : "Skipped")}: {skippedCount}, Errors: {errorCount}");
                lastReportTime = DateTime.Now;
            }
        }

        var finalAction = _dryRun ? "Previewed" : "Processed";
        Console.WriteLine($"{finalAction}: {processedCount}/{totalFiles} files (100.0%) - {(_dryRun ? "Would copy" : "Copied")}: {copiedCount}, {(_dryRun ? "Would skip" : "Skipped")}: {skippedCount}, Errors: {errorCount}");

        return (copiedCount, skippedCount, errorCount);
    }

    private CopyOperation? CopySingleFile(string sourceFile, string targetFolderPattern, StreamWriter logWriter, int fileIndex, int totalFiles)
    {
        try
        {
            var fileInfo = new FileInfo(sourceFile);
            var targetFullPath = PathHelper.BuildTargetPath(sourceFile, fileInfo, targetFolderPattern);
            var originalTargetPath = targetFullPath;
            
            var (finalTargetPath, collisionResult) = ResolveTargetPathWithCollisionHandling(sourceFile, targetFullPath);
            
            if (!_dryRun)
            {
                PathHelper.EnsureDirectoryExists(finalTargetPath);
            }
            
            var operation = CopyFileIfNeeded(sourceFile, finalTargetPath, fileInfo, collisionResult, originalTargetPath);
            LogCopyOperation(operation, logWriter, fileIndex, totalFiles);
            
            return operation;
        }
        catch (Exception ex)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            logWriter.WriteLine($"[{timestamp}] [{fileIndex}/{totalFiles}] ERROR: Failed to {(_dryRun ? "preview" : "copy")} '{sourceFile}' - {ex.Message}");
            Console.WriteLine($"Error {(_dryRun ? "previewing" : "copying")} file '{sourceFile}': {ex.Message}");
            return null;
        }
    }

    private (string targetPath, CopyResult? collisionResult) ResolveTargetPathWithCollisionHandling(string sourceFile, string targetFullPath)
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

        if (FileHashHelper.AreFilesIdentical(sourceFile, targetFullPath))
        {
            return (targetFullPath, CopyResult.SkippedSameHash);
        }

        var newPath = GetUniqueTargetPath(sourceFile, targetFullPath, out var result);
        return (newPath, result);
    }

    private string GetUniqueTargetPath(string sourceFile, string originalTargetPath, out CopyResult? skipResult)
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
            
            if (sourceInfo.Length == targetInfo.Length && FileHashHelper.AreFilesIdentical(sourceFile, newTargetPath))
            {
                skipResult = CopyResult.SkippedSameHash;
                return newTargetPath;
            }

            counter++;
        } while (counter < 10000);

        throw new InvalidOperationException($"Too many file collisions for: {originalTargetPath}");
    }

    private CopyOperation CopyFileIfNeeded(string sourceFile, string targetFullPath, FileInfo sourceFileInfo, CopyResult? collisionResult, string originalTargetPath)
    {
        var fileSize = sourceFileInfo.Length;
        
        if (collisionResult == CopyResult.SkippedSameHash)
        {
            return new CopyOperation(CopyResult.SkippedSameHash, sourceFile, targetFullPath, fileSize, originalTargetPath);
        }
        
        if (collisionResult == CopyResult.SkippedDifferentHash)
        {
            if (!_dryRun)
            {
                File.Copy(sourceFile, targetFullPath, overwrite: false);
                File.SetLastWriteTime(targetFullPath, sourceFileInfo.LastWriteTime);
            }
            return new CopyOperation(CopyResult.Copied, sourceFile, targetFullPath, fileSize, originalTargetPath);
        }
        
        if (!File.Exists(targetFullPath))
        {
            if (!_dryRun)
            {
                File.Copy(sourceFile, targetFullPath, overwrite: false);
                File.SetLastWriteTime(targetFullPath, sourceFileInfo.LastWriteTime);
            }
            return new CopyOperation(CopyResult.Copied, sourceFile, targetFullPath, fileSize);
        }
        
        return new CopyOperation(CopyResult.SkippedSameHash, sourceFile, targetFullPath, fileSize);
    }

    private void LogCopyOperation(CopyOperation? operation, StreamWriter logWriter, int fileIndex, int totalFiles)
    {
        if (operation == null) return;
        
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var sizeKB = operation.FileSize / 1024.0;
        var sizeMB = sizeKB / 1024.0;
        var sizeStr = sizeMB >= 1 ? $"{sizeMB:F2} MB" : $"{sizeKB:F2} KB";
        var actionPrefix = _dryRun ? "[PREVIEW] " : "";
        
        switch (operation.Result)
        {
            case CopyResult.Copied:
                if (operation.OriginalTargetPath != null && operation.OriginalTargetPath != operation.TargetPath)
                {
                    logWriter.WriteLine($"[{timestamp}] [{fileIndex}/{totalFiles}] {actionPrefix}COPY (DUPLICATE - DIFFERENT HASH)");
                    logWriter.WriteLine($"  Source: {operation.SourcePath} ({sizeStr})");
                    logWriter.WriteLine($"  Original Target: {operation.OriginalTargetPath}");
                    logWriter.WriteLine($"  Actual Target: {operation.TargetPath}");
                }
                else
                {
                    logWriter.WriteLine($"[{timestamp}] [{fileIndex}/{totalFiles}] {actionPrefix}COPY");
                    logWriter.WriteLine($"  Source: {operation.SourcePath} ({sizeStr})");
                    logWriter.WriteLine($"  Target: {operation.TargetPath}");
                }
                break;
                
            case CopyResult.SkippedSameHash:
                logWriter.WriteLine($"[{timestamp}] [{fileIndex}/{totalFiles}] {actionPrefix}SKIP (DUPLICATE - SAME HASH)");
                logWriter.WriteLine($"  Source: {operation.SourcePath} ({sizeStr})");
                logWriter.WriteLine($"  Target: {operation.TargetPath}");
                break;
        }
        
        logWriter.WriteLine();
    }
}

