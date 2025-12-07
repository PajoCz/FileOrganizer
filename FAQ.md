# Frequently Asked Questions (FAQ)

## General Questions

### What does this application do?
File Organizer analyzes and copies files (primarily photos and videos) into a date-based folder structure with intelligent duplicate detection using SHA256 hashing.

### Is it safe to use?
Yes! The application only **copies** files, never modifies or deletes the originals. Your source files remain untouched.

### What operating systems are supported?
Windows, Linux, and macOS - any platform that supports .NET 8.

## Configuration

### How do I change which file types to process?
Edit the `FileExtensions` key in `appsettings.json`:
```json
"FileExtensions": ".jpg;.png;.mp4;.avi"
```

### Can I use different date formats?
Yes! You can customize the pattern using any valid .NET date format:
- `{FileDate:yyyy}` - Year (2024)
- `{FileDate:MM}` - Month (03)
- `{FileDate:dd}` - Day (15)
- `{FileDate:HH}` - Hour (14)
- `{FileDate:mm}` - Minute (30)
- `{FileDate:ss}` - Second (22)

### What if I want to organize by creation date instead of modified date?
Currently, the application uses `LastWriteTime`. To use creation date, you would need to modify the code to use `CreationTime` instead.

## Usage

### Should I run --analyze first?
**Yes!** Always run `--analyze` first to see:
- How many files will be processed
- What file types were found
- Verify your source folder is correct

### Can I run the copy operation multiple times?
Yes! The duplicate detection will skip files that were already copied (same hash). Only new or modified files will be copied.

### What happens if I interrupt the copy operation?
You can safely restart it. Already-copied files will be detected as duplicates and skipped.

## Duplicate Handling

### How does duplicate detection work?
The application compares files using SHA256 hash:
1. First checks file size (fast)
2. If sizes match, calculates SHA256 hash (slower)
3. Compares hashes to determine if files are identical

### What if I have two different photos with the same filename?
The application will copy both:
- First file: `photo.jpg`
- Second file: `photo_1.jpg`
- Third file: `photo_2.jpg`
(Only if the files have different content/hash)

### Will it skip identical files from different folders?
Yes! If two files have the same hash, only the first one will be copied. Subsequent identical files will be skipped regardless of their source location.

## Performance

### How fast is it?
Performance depends on:
- Number of files
- File sizes
- Storage speed (HDD vs SSD)
- Number of duplicates (hash calculation takes time)

Typical speed: 100-500 files per minute for mostly unique files.

### Why is it slower when there are duplicates?
Hash calculation requires reading the entire file. The application optimizes by:
1. Comparing file sizes first (instant)
2. Only calculating hash if sizes match
3. Skipping hash for new files

### Can it process files in parallel?
Not in the current version. This is a planned feature for future releases.

## Errors and Troubleshooting

### "SourceFolder not configured"
Create `appsettings.json` from `appsettings.json.example` and edit the paths.

### "Source folder does not exist"
Check that the path in `SourceFolder` is correct and accessible.

### "Access denied" errors
Make sure you have:
- Read permission for source folder
- Write permission for target folder
- No files are locked by other applications

### Some files are skipped with errors
Check the log file for specific error messages. Common causes:
- File in use by another program
- Insufficient permissions
- Path too long (Windows)
- Disk full

### Path too long error (Windows)
Windows has a 260-character path limit. Solutions:
- Use shorter target path pattern
- Enable long path support in Windows
- Move files closer to drive root

## Output Files

### Where is the analysis report saved?
In the current directory as `file-analysis.txt` (configurable in `appsettings.json`).

### Where is the copy log saved?
In the current directory as `copy-log.txt` (configurable in `appsettings.json`).

### Can I change the log format?
The format is currently fixed, but you can modify `LogCopyOperation` method in `Program.cs`.

## Advanced Usage

### Can I move files instead of copying?
Not in the current version. This is planned for future releases. Currently, you need to:
1. Run copy operation
2. Verify files in target
3. Manually delete source files if desired

### Can I filter by date range?
Not currently. You could:
1. Run the copy operation
2. Use file system tools to filter by date after

### Can it read EXIF data from photos?
Not in the current version. Currently uses file system dates. EXIF support is planned.

### Can I customize the numbering for duplicates?
Currently uses `_1`, `_2`, etc. You can modify `GetUniqueTargetPath` in `Program.cs` to change this.

## Contributing

### How can I contribute?
See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

### Where do I report bugs?
Create an issue on GitHub with:
- Description of the problem
- Steps to reproduce
- Expected vs actual behavior
- Log file excerpts

### Can I suggest features?
Absolutely! Create an issue with your feature request and use case.

## Still Have Questions?

Create an issue on GitHub or check the documentation:
- [README.md](README.md) - Full documentation
- [QUICKSTART.md](QUICKSTART.md) - Getting started guide
- [EXAMPLES.md](EXAMPLES.md) - Usage examples
