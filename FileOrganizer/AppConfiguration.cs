using Microsoft.Extensions.Configuration;

namespace FileOrganizer;

public class AppConfiguration
{
    public string SourceFolder { get; }
    public string OutputFileName { get; }
    public string FileExtensions { get; }
    public string TargetFolderPattern { get; }
    public string CopyLogFileName { get; }
    public string DuplicatesLogFileName { get; }
    public string CleanupLogFileName { get; }
    public string FileNamePrefix { get; }

    public AppConfiguration(IConfiguration configuration)
    {
        SourceFolder = configuration["SourceFolder"] 
            ?? throw new InvalidOperationException("SourceFolder not configured");
        
        var rawOutputFileName = configuration["OutputFileName"] 
            ?? throw new InvalidOperationException("OutputFileName not configured");
        OutputFileName = PathHelper.ExpandDateTimeMacros(rawOutputFileName);
        
        FileExtensions = configuration["FileExtensions"] 
            ?? throw new InvalidOperationException("FileExtensions not configured");
        
        TargetFolderPattern = configuration["TargetFolderPattern"] 
            ?? throw new InvalidOperationException("TargetFolderPattern not configured");
        
        var rawCopyLogFileName = configuration["CopyLogFileName"] ?? "copy-log.txt";
        CopyLogFileName = PathHelper.ExpandDateTimeMacros(rawCopyLogFileName);
        
        var rawDuplicatesLogFileName = configuration["DuplicatesLogFileName"] ?? "duplicates-log.txt";
        DuplicatesLogFileName = PathHelper.ExpandDateTimeMacros(rawDuplicatesLogFileName);
        
        var rawCleanupLogFileName = configuration["CleanupLogFileName"] ?? "cleanup-log_{DateTime:yyyyMMdd_HHmmss}.txt";
        CleanupLogFileName = PathHelper.ExpandDateTimeMacros(rawCleanupLogFileName);
        
        // Extract prefix pattern before {FileName}
        FileNamePrefix = ExtractFileNamePrefix(TargetFolderPattern);
    }

    private string ExtractFileNamePrefix(string pattern)
    {
        // Find {FileName} in pattern
        var fileNameIndex = pattern.IndexOf("{FileName}");
        if (fileNameIndex == -1)
            return string.Empty;

        // Find last path separator before {FileName}
        var lastSeparator = pattern.LastIndexOf('\\', fileNameIndex);
        if (lastSeparator == -1)
            lastSeparator = pattern.LastIndexOf('/', fileNameIndex);

        if (lastSeparator == -1)
            return string.Empty;

        // Extract everything between last separator and {FileName}
        var prefixPart = pattern.Substring(lastSeparator + 1, fileNameIndex - lastSeparator - 1);
        
        // Replace any {FileDate:...} macros with regex pattern
        prefixPart = System.Text.RegularExpressions.Regex.Replace(
            prefixPart, 
            @"\{FileDate:([^}]+)\}", 
            match => 
            {
                var format = match.Groups[1].Value;
                
                // Replace each format character with \d and keep separators as-is
                var result = new System.Text.StringBuilder();
                foreach (var c in format)
                {
                    if (char.IsLetter(c))
                    {
                        result.Append(@"\d");
                    }
                    else
                    {
                        // Escape special regex characters
                        if (c == '.' || c == '+' || c == '*' || c == '?' || c == '[' || c == ']' || 
                            c == '(' || c == ')' || c == '{' || c == '}' || c == '|' || c == '\\' || c == '^' || c == '$')
                        {
                            result.Append('\\');
                        }
                        result.Append(c);
                    }
                }
                
                return result.ToString();
            });

        return prefixPart;
    }
}
