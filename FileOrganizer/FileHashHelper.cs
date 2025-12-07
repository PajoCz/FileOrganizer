using System.Security.Cryptography;

namespace FileOrganizer;

public class FileHashHelper
{
    public static bool AreFilesIdentical(string file1, string file2)
    {
        var file1Info = new FileInfo(file1);
        var file2Info = new FileInfo(file2);

        if (file1Info.Length != file2Info.Length)
        {
            return false;
        }

        var hash1 = ComputeFileHash(file1);
        var hash2 = ComputeFileHash(file2);

        return hash1.SequenceEqual(hash2);
    }

    private static byte[] ComputeFileHash(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(stream);
    }
}
