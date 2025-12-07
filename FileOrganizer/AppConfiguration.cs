using Microsoft.Extensions.Configuration;

namespace FileOrganizer;

public class AppConfiguration
{
    public string SourceFolder { get; }
    public string OutputFileName { get; }
    public string FileExtensions { get; }
    public string TargetFolderPattern { get; }
    public string CopyLogFileName { get; }

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
    }
}
