namespace FileOrganizer;

public enum CopyResult
{
    Copied,
    SkippedSameHash,
    SkippedDifferentHash
}

public record CopyOperation(CopyResult Result, string SourcePath, string TargetPath, long FileSize, string? OriginalTargetPath = null);
