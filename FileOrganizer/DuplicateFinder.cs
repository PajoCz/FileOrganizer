namespace FileOrganizer;

public class DuplicateFinder
{
    private readonly string _fileNamePrefixPattern;

    public DuplicateFinder(string fileNamePrefixPattern)
    {
        _fileNamePrefixPattern = fileNamePrefixPattern;
    }

    public void FindDuplicates(string targetFolder, string duplicatesLogFileName)
    {
        Console.WriteLine($"Searching for duplicates in: {targetFolder}");
        Console.WriteLine($"Using filename prefix pattern: {_fileNamePrefixPattern}");
        
        if (!Directory.Exists(targetFolder))
        {
            Console.WriteLine($"Error: Target folder '{targetFolder}' does not exist!");
            return;
        }

        var allFiles = Directory.GetFiles(targetFolder, "*.*", SearchOption.AllDirectories);
        Console.WriteLine($"Found {allFiles.Length} files to analyze...");

        var duplicates = new List<DuplicateGroup>();
        var filesByHash = new Dictionary<string, List<FileRecord>>();
        var processedCount = 0;
        var lastReportTime = DateTime.Now;

        // Group files by hash
        foreach (var filePath in allFiles)
        {
            processedCount++;
            
            var fileInfo = new FileInfo(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
            var originalName = ExtractOriginalFileName(fileName);
            
            if (string.IsNullOrEmpty(originalName))
            {
                Console.WriteLine($"Warning: Could not extract original name from: {fileName}");
                continue;
            }

            // Include extension in the key to differentiate files with same name but different extensions
            var key = $"{originalName}_{fileExtension}_{fileInfo.Length}";
            
            if (!filesByHash.ContainsKey(key))
                filesByHash[key] = new List<FileRecord>();
            
            filesByHash[key].Add(new FileRecord(filePath, fileInfo.Length, originalName));

            if ((DateTime.Now - lastReportTime).TotalSeconds >= 1)
            {
                Console.WriteLine($"Analyzed: {processedCount}/{allFiles.Length} files ({processedCount * 100.0 / allFiles.Length:F1}%)");
                lastReportTime = DateTime.Now;
            }
        }

        Console.WriteLine($"Analyzed: {processedCount}/{allFiles.Length} files (100.0%)");
        Console.WriteLine($"Checking for duplicates with same size...");

        // Check for duplicates with same size
        var duplicateCount = 0;
        foreach (var group in filesByHash.Values.Where(g => g.Count > 1))
        {
            var hashGroups = new Dictionary<string, List<FileRecord>>();
            
            foreach (var file in group)
            {
                var hash = ComputeFileHash(file.FilePath);
                var hashStr = BitConverter.ToString(hash).Replace("-", "");
                
                if (!hashGroups.ContainsKey(hashStr))
                    hashGroups[hashStr] = new List<FileRecord>();
                
                hashGroups[hashStr].Add(file);
            }

            foreach (var hashGroup in hashGroups.Values.Where(g => g.Count > 1))
            {
                duplicates.Add(new DuplicateGroup(hashGroup.ToList()));
                duplicateCount++;
            }
        }

        WriteDuplicatesLog(duplicatesLogFileName, duplicates);

        Console.WriteLine($"\nDuplicate detection complete!");
        Console.WriteLine($"Found {duplicates.Count} duplicate groups with {duplicates.Sum(d => d.Files.Count)} total files");
        Console.WriteLine($"Detailed log saved to: {duplicatesLogFileName}");
    }

    private string? ExtractOriginalFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(_fileNamePrefixPattern))
            return fileName;

        // Use regex to remove the prefix pattern
        var pattern = "^" + _fileNamePrefixPattern;
        var originalName = System.Text.RegularExpressions.Regex.Replace(fileName, pattern, "");
        
        return string.IsNullOrWhiteSpace(originalName) ? null : originalName;
    }

    private byte[] ComputeFileHash(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        return sha256.ComputeHash(stream);
    }

    private void WriteDuplicatesLog(string logFileName, List<DuplicateGroup> duplicates)
    {
        using var writer = new StreamWriter(logFileName, append: false);
        writer.WriteLine($"Duplicate Files Report - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        writer.WriteLine(new string('=', 100));
        writer.WriteLine($"Total duplicate groups found: {duplicates.Count}");
        writer.WriteLine($"Total duplicate files: {duplicates.Sum(d => d.Files.Count)}");
        writer.WriteLine();

        var groupNumber = 1;
        foreach (var group in duplicates.OrderByDescending(g => g.Files[0].FileSize))
        {
            writer.WriteLine($"=== Duplicate Group #{groupNumber} ===");
            writer.WriteLine($"Original Name: {group.Files[0].OriginalName}");
            writer.WriteLine($"File Size: {FormatBytes(group.Files[0].FileSize)}");
            writer.WriteLine($"Number of copies: {group.Files.Count}");
            writer.WriteLine();

            // Analyze all files and determine which to keep
            var fileAnalysis = new List<(string FilePath, DateTime? DateInName, DateTime ActualDate, bool Match)>();
            
            foreach (var file in group.Files)
            {
                var fileInfo = new FileInfo(file.FilePath);
                var dateInName = ExtractDateFromFileName(Path.GetFileNameWithoutExtension(file.FilePath));
                var actualDate = fileInfo.LastWriteTime;
                var match = dateInName.HasValue && 
                           Math.Abs((dateInName.Value - actualDate).TotalSeconds) < 2;
                
                fileAnalysis.Add((file.FilePath, dateInName, actualDate, match));
            }

            // Find files with matching dates
            var matchingFiles = fileAnalysis.Where(f => f.Match).ToList();
            
            // If we have matching files, keep only the first one alphabetically
            // If no matching files, keep the first one alphabetically as safety
            var fileToKeep = matchingFiles.Any() 
                ? matchingFiles.OrderBy(f => f.FilePath).First().FilePath
                : fileAnalysis.OrderBy(f => f.FilePath).First().FilePath;

            // Write analysis for each file
            foreach (var analysis in fileAnalysis.OrderBy(f => f.FilePath))
            {
                var shouldKeep = analysis.FilePath == fileToKeep;
                
                writer.WriteLine($"  Path: {analysis.FilePath}");
                writer.WriteLine($"  Date in filename: {(analysis.DateInName.HasValue ? analysis.DateInName.Value.ToString("yyyy-MM-dd HH:mm:ss") : "N/A")}");
                writer.WriteLine($"  Actual file date: {analysis.ActualDate:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine($"  Date match: {(analysis.Match ? "YES" : "NO")}");
                writer.WriteLine($"  Action: {(shouldKeep ? "KEEP" : "DELETE (DUPLICATE)")}");
                writer.WriteLine();
            }

            writer.WriteLine();
            groupNumber++;
        }
    }

    private DateTime? ExtractDateFromFileName(string fileName)
    {
        // Try to find yyyyMMdd_HHmmss pattern at the beginning
        var match = System.Text.RegularExpressions.Regex.Match(fileName, @"^(\d{8})_(\d{6})");
        
        if (!match.Success)
            return null;

        try
        {
            var dateStr = match.Groups[1].Value;
            var timeStr = match.Groups[2].Value;
            
            var year = int.Parse(dateStr.Substring(0, 4));
            var month = int.Parse(dateStr.Substring(4, 2));
            var day = int.Parse(dateStr.Substring(6, 2));
            var hour = int.Parse(timeStr.Substring(0, 2));
            var minute = int.Parse(timeStr.Substring(2, 2));
            var second = int.Parse(timeStr.Substring(4, 2));
            
            return new DateTime(year, month, day, hour, minute, second);
        }
        catch
        {
            return null;
        }
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
}

public record FileRecord(string FilePath, long FileSize, string OriginalName);
public record DuplicateGroup(List<FileRecord> Files);
