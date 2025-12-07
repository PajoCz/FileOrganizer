namespace FileOrganizer;

public class FileCopier
{
    private bool _dryRun;
    
    public void CopyFiles(string sourceFolder, string fileExtensions, string targetFolderPattern, string copyLogFileName, bool dryRun = false)
    {
        _dryRun = dryRun;
        
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
        
        var (copiedCount, skippedCount) = ProcessFileCopying(filesToCopy, targetFolderPattern, logWriter);

        logWriter.WriteLine();
        logWriter.WriteLine(new string('=', 100));
        logWriter.WriteLine($"Operation completed: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        logWriter.WriteLine($"Total files processed: {filesToCopy.Length}");
        logWriter.WriteLine($"Files {(_dryRun ? "would be " : "")}copied: {copiedCount}");
        logWriter.WriteLine($"Files {(_dryRun ? "would be " : "")}skipped: {skippedCount}");
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
            Console.WriteLine($"Copy complete! Files copied: {copiedCount}, Skipped: {skippedCount}");
        }
        Console.WriteLine($"Detailed log saved to: {copyLogFileName}");
    }

    private string[] GetFilesToCopy(string sourceFolder, HashSet<string> allowedExtensions)
    {
        var allFiles = Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories);
        return allFiles.Where(f => allowedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant())).ToArray();
    }

    private (int copiedCount, int skippedCount) ProcessFileCopying(string[] filesToCopy, string targetFolderPattern, StreamWriter logWriter)
    {
        var totalFiles = filesToCopy.Length;
        var processedCount = 0;
        var copiedCount = 0;
        var skippedCount = 0;
        var lastReportTime = DateTime.Now;

        Console.WriteLine($"Found {totalFiles} files to {(_dryRun ? "preview" : "copy")}...");

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
                var action = _dryRun ? "Previewed" : "Processed";
                Console.WriteLine($"{action}: {processedCount}/{totalFiles} files ({processedCount * 100.0 / totalFiles:F1}%) - {(_dryRun ? "Would copy" : "Copied")}: {copiedCount}, {(_dryRun ? "Would skip" : "Skipped")}: {skippedCount}");
                lastReportTime = DateTime.Now;
            }
        }

        var finalAction = _dryRun ? "Previewed" : "Processed";
        Console.WriteLine($"{finalAction}: {processedCount}/{totalFiles} files (100.0%) - {(_dryRun ? "Would copy" : "Copied")}: {copiedCount}, {(_dryRun ? "Would skip" : "Skipped")}: {skippedCount}");

        return (copiedCount, skippedCount);
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
