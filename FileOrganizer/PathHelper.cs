using System.Text.RegularExpressions;

namespace FileOrganizer;

public class PathHelper
{
    public static string ExpandDateTimeMacros(string path)
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

    public static string BuildTargetPath(string sourceFile, FileInfo fileInfo, string targetFolderPattern)
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

    public static void EnsureDirectoryExists(string targetFullPath)
    {
        var targetDirectory = Path.GetDirectoryName(targetFullPath);
        if (!string.IsNullOrEmpty(targetDirectory) && !Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }
    }

    public static HashSet<string> ParseFileExtensions(string fileExtensions)
    {
        return fileExtensions
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(ext => ext.Trim().ToLowerInvariant())
            .ToHashSet();
    }
}
