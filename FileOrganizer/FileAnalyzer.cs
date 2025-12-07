namespace FileOrganizer;

public class FileAnalyzer
{
    public void AnalyzeFiles(string sourceFolder, string outputFileName)
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

    private Dictionary<string, int> CountFilesByExtension(string[] files)
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

    private void SaveAnalysisReport(string outputFileName, string sourceFolder, string[] files, Dictionary<string, int> extensionCounts)
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
}
