namespace FileOrganizer;

public class DuplicateCleaner
{
    public void CleanDuplicates(string duplicatesLogFileName, string cleanupLogFileName, bool dryRun = false)
    {
        Console.WriteLine($"Reading duplicates from: {duplicatesLogFileName}");
        
        if (!File.Exists(duplicatesLogFileName))
        {
            Console.WriteLine($"Error: Duplicates log file '{duplicatesLogFileName}' does not exist!");
            Console.WriteLine("Run --find-duplicates first to generate the log file.");
            return;
        }

        var duplicateGroups = ParseDuplicatesLog(duplicatesLogFileName);
        
        if (duplicateGroups.Count == 0)
        {
            Console.WriteLine("No duplicates found in the log file.");
            return;
        }

        if (dryRun)
        {
            Console.WriteLine("=== DRY RUN MODE - No files will be actually deleted ===");
            Console.WriteLine();
        }

        Console.WriteLine($"Found {duplicateGroups.Count} duplicate groups");
        Console.WriteLine($"Processing...\n");

        var totalDeleted = 0;
        var totalKept = 0;
        var totalBytes = 0L;
        var startTime = DateTime.Now;

        using var logWriter = new StreamWriter(cleanupLogFileName, append: false);
        logWriter.WriteLine($"Duplicate Cleanup Log{(dryRun ? " (DRY RUN)" : "")} - Started: {startTime:yyyy-MM-dd HH:mm:ss}");
        logWriter.WriteLine($"Source duplicates log: {duplicatesLogFileName}");
        logWriter.WriteLine(new string('=', 100));
        if (dryRun)
        {
            logWriter.WriteLine("DRY RUN MODE - No files were actually deleted, this is a preview only");
            logWriter.WriteLine(new string('=', 100));
        }
        logWriter.WriteLine();

        foreach (var group in duplicateGroups)
        {
            Console.WriteLine($"Processing group: {group.OriginalName}");
            logWriter.WriteLine($"=== Group: {group.OriginalName} ===");
            
            foreach (var fileAction in group.FileActions)
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                
                if (!File.Exists(fileAction.FilePath))
                {
                    Console.WriteLine($"  SKIP: File not found: {fileAction.FilePath}");
                    logWriter.WriteLine($"[{timestamp}] SKIP (FILE NOT FOUND): {fileAction.FilePath}");
                    continue;
                }

                var fileInfo = new FileInfo(fileAction.FilePath);
                var fileName = Path.GetFileName(fileAction.FilePath);
                var fileSize = FormatBytes(fileInfo.Length);

                if (fileAction.Action == "KEEP")
                {
                    totalKept++;
                    Console.WriteLine($"  KEEP: {fileName}");
                    logWriter.WriteLine($"[{timestamp}] KEEP: {fileAction.FilePath} ({fileSize})");
                }
                else // DELETE (DUPLICATE)
                {
                    totalBytes += fileInfo.Length;
                    Console.WriteLine($"  {(dryRun ? "WOULD DELETE" : "DELETE")}: {fileName}");
                    
                    if (!dryRun)
                    {
                        try
                        {
                            File.Delete(fileAction.FilePath);
                            totalDeleted++;
                            logWriter.WriteLine($"[{timestamp}] DELETED: {fileAction.FilePath} ({fileSize})");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"  ERROR: Failed to delete {fileAction.FilePath}: {ex.Message}");
                            logWriter.WriteLine($"[{timestamp}] ERROR: {fileAction.FilePath} - {ex.Message}");
                        }
                    }
                    else
                    {
                        totalDeleted++;
                        logWriter.WriteLine($"[{timestamp}] WOULD DELETE: {fileAction.FilePath} ({fileSize})");
                    }
                }
            }

            logWriter.WriteLine();
            Console.WriteLine();
        }

        var endTime = DateTime.Now;
        var duration = endTime - startTime;

        logWriter.WriteLine(new string('=', 100));
        logWriter.WriteLine($"Operation completed: {endTime:yyyy-MM-dd HH:mm:ss}");
        logWriter.WriteLine($"Duration: {FormatDuration(duration)}");
        logWriter.WriteLine();
        logWriter.WriteLine("=== CLEANUP SUMMARY ===");
        logWriter.WriteLine($"Duplicate groups processed: {duplicateGroups.Count}");
        logWriter.WriteLine($"Files {(dryRun ? "would be " : "")}deleted: {totalDeleted}");
        logWriter.WriteLine($"Files kept: {totalKept}");
        logWriter.WriteLine($"Space {(dryRun ? "would be " : "")}freed: {FormatBytes(totalBytes)}");
        
        if (dryRun)
        {
            logWriter.WriteLine();
            logWriter.WriteLine("NOTE: This was a DRY RUN. No files were actually deleted.");
        }

        Console.WriteLine("========================================================");
        Console.WriteLine("|                  CLEANUP SUMMARY                     |");
        Console.WriteLine("========================================================");
        Console.WriteLine($"| Duplicate groups processed: {duplicateGroups.Count,10}              |");
        Console.WriteLine($"| Files {(dryRun ? "would be " : "")}deleted:        {totalDeleted,10}              |");
        Console.WriteLine($"| Files kept:                 {totalKept,10}              |");
        Console.WriteLine($"| Space {(dryRun ? "would be " : "")}freed:         {FormatBytes(totalBytes),20} |");
        Console.WriteLine("========================================================");

        Console.WriteLine($"\nDetailed cleanup log saved to: {cleanupLogFileName}");

        if (dryRun)
        {
            Console.WriteLine("\nThis was a DRY RUN. No files were actually deleted.");
            Console.WriteLine("Run without --dry-run to perform the actual cleanup.");
        }
    }

    private List<DuplicateGroupInfo> ParseDuplicatesLog(string logFileName)
    {
        var groups = new List<DuplicateGroupInfo>();
        var lines = File.ReadAllLines(logFileName);
        
        string? currentOriginalName = null;
        var currentFileActions = new List<FileAction>();
        string? currentFilePath = null;
        string? currentAction = null;

        foreach (var line in lines)
        {
            if (line.StartsWith("Original Name:"))
            {
                currentOriginalName = line.Substring("Original Name:".Length).Trim();
            }
            else if (line.StartsWith("  Path:"))
            {
                // Save previous file if exists
                if (currentFilePath != null && currentAction != null)
                {
                    currentFileActions.Add(new FileAction(currentFilePath, currentAction));
                }
                
                currentFilePath = line.Substring("  Path:".Length).Trim();
                currentAction = null;
            }
            else if (line.StartsWith("  Action:"))
            {
                currentAction = line.Substring("  Action:".Length).Trim();
            }
            else if (line.StartsWith("=== Duplicate Group"))
            {
                // Save previous file if exists
                if (currentFilePath != null && currentAction != null)
                {
                    currentFileActions.Add(new FileAction(currentFilePath, currentAction));
                    currentFilePath = null;
                    currentAction = null;
                }
                
                // Save previous group if exists
                if (currentOriginalName != null && currentFileActions.Count > 0)
                {
                    groups.Add(new DuplicateGroupInfo(currentOriginalName, currentFileActions.ToList()));
                    currentFileActions.Clear();
                }
                
                currentOriginalName = null;
            }
        }

        // Save last file and group
        if (currentFilePath != null && currentAction != null)
        {
            currentFileActions.Add(new FileAction(currentFilePath, currentAction));
        }
        
        if (currentOriginalName != null && currentFileActions.Count > 0)
        {
            groups.Add(new DuplicateGroupInfo(currentOriginalName, currentFileActions.ToList()));
        }

        return groups;
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
}

public record FileAction(string FilePath, string Action);
public record DuplicateGroupInfo(string OriginalName, List<FileAction> FileActions);
