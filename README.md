# File Organizer

A .NET 8 console application for analyzing and organizing files (photos and videos) by date with intelligent duplicate detection using SHA256 hashing.

## Features

- ?? **File Analysis** - Analyze files by extension and generate detailed reports
- ?? **Smart File Organization** - Copy files to date-based folder structure
- ?? **Duplicate Detection** - SHA256 hash-based duplicate detection
- ?? **Collision Handling** - Automatic numbering for files with same name but different content
- ?? **Detailed Logging** - Complete operation log with timestamps and file sizes
- ? **Performance Optimized** - Hash calculation only when necessary

## Requirements

- .NET 8.0 SDK or later
- Windows, Linux, or macOS

## Installation

1. Clone the repository:
```bash
git clone https://github.com/yourusername/FileOrganizer.git
cd FileOrganizer
```

2. Build the project:
```bash
dotnet build
```

## Configuration

Edit `appsettings.json` to configure the application:

```json
{
  "SourceFolder": "c:\\path\\to\\source",
  "OutputFileName": "file-analysis_{DateTime:yyyyMMdd_HHmmss}.txt",
  "FileExtensions": ".jpg;.jpeg;.png;.bmp;.gif;.mp4;.avi;.mov;.mpeg;.mpg;.wmv;.3gp;.vob;.ts;.mkv",
  "TargetFolderPattern": "c:\\path\\to\\target\\{FileDate:yyyy}\\{FileDate:MM}\\{FileDate:yyyyMMdd_HHmmss}_{FileName}.{FileExtension}",
  "CopyLogFileName": "copy-log_{DateTime:yyyyMMdd_HHmmss}.txt"
}
```

### Configuration Keys

| Key | Description | Example |
|-----|-------------|---------|
| `SourceFolder` | Source directory containing files to process | `c:\\Photos` |
| `OutputFileName` | Output file name for analysis report (supports DateTime macros) | `file-analysis_{DateTime:yyyyMMdd_HHmmss}.txt` |
| `FileExtensions` | Semicolon-separated list of file extensions to process | `.jpg;.png;.mp4` |
| `TargetFolderPattern` | Target path pattern with macros (see below) | `c:\\Organized\\{FileDate:yyyy}\\{FileDate:MM}\\...` |
| `CopyLogFileName` | Log file name for copy operations (supports DateTime macros) | `copy-log_{DateTime:yyyyMMdd_HHmmss}.txt` |

### DateTime Macros (for Output and Log File Names)

Use these macros in `OutputFileName` and `CopyLogFileName` to include current date/time in file names (prevents overwriting).

**Format:** `{DateTime:format}` where `format` is any valid [.NET DateTime format string](https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings)

**Common Examples:**

| Macro | Description | Example Output |
|-------|-------------|----------------|
| `{DateTime:yyyy}` | Year (4 digits) | `2024` |
| `{DateTime:MM}` | Month (2 digits) | `03` |
| `{DateTime:dd}` | Day (2 digits) | `15` |
| `{DateTime:HH}` | Hour (2 digits, 24h) | `14` |
| `{DateTime:mm}` | Minute (2 digits) | `30` |
| `{DateTime:ss}` | Second (2 digits) | `22` |
| `{DateTime:yyyyMMdd}` | Date compact | `20240315` |
| `{DateTime:HHmmss}` | Time compact | `143022` |
| `{DateTime:yyyyMMdd_HHmmss}` | Full timestamp | `20240315_143022` |
| `{DateTime:yyyy-MM-dd}` | ISO date | `2024-03-15` |
| `{DateTime:ddMMyyyy}` | European date | `15032024` |

**You can use ANY .NET DateTime format!** Examples:
- `{DateTime:yyyy-MM-dd_HH-mm-ss}` ? `2024-03-15_14-30-22`
- `{DateTime:yyyyMMdd}` ? `20240315`
- `{DateTime:HH-mm}` ? `14-30`


**Example Configuration:**
```json
{
  "OutputFileName": "file-analysis_{DateTime:yyyyMMdd_HHmmss}.txt",
  "CopyLogFileName": "copy-log_{DateTime:yyyyMMdd_HHmmss}.txt"
}
```

**Results in:**
```
file-analysis_20240315_143022.txt
copy-log_20240315_143022.txt
```

### Target Folder Pattern Macros (for Organizing Files)

Use these macros in `TargetFolderPattern` based on file's LastWriteTime:

| Macro | Description | Example Output |
|-------|-------------|----------------|
| `{FileDate:yyyy}` | Year (4 digits) | `2024` |
| `{FileDate:MM}` | Month (2 digits) | `03` |
| `{FileDate:yyyyMMdd_HHmmss}` | Full timestamp | `20240315_143022` |
| `{FileName}` | Original file name without extension | `IMG_001` |
| `{FileExtension}` | File extension without dot | `jpg` |

**Example Pattern:**
```
c:\Organized\{FileDate:yyyy}\{FileDate:MM}\{FileDate:yyyyMMdd_HHmmss}_{FileName}.{FileExtension}
```

**Results in:**
```
c:\Organized\2024\03\20240315_143022_IMG_001.jpg
```

## Usage

### Step 1: Analyze Files

First, analyze the source folder to see what files will be processed:

```bash
dotnet run --analyze
```

This generates a report (`file-analysis.txt`) showing:
- Total number of files
- File count by extension
- Source folder path

**Example output:**
```
File Analysis Report - 2024-03-15 14:30:22
Source Folder: c:\Photos
Total Files: 1500
--------------------------------------------------

.jpg                          850 files
.mp4                          320 files
.png                          230 files
.mov                          100 files
```

### Step 2: Copy Files

After reviewing the analysis, copy files to the organized structure:

```bash
dotnet run --copy
```

This will:
1. Copy files matching configured extensions
2. Organize them using the date-based pattern
3. Detect and handle duplicates
4. Generate detailed log file

## Duplicate Handling

The application intelligently handles duplicate files:

### Scenario 1: Identical File (Same Hash)
**Action:** Skip - file already exists
```
[2024-03-15 14:30:24] [2/500] SKIP (DUPLICATE - SAME HASH)
  Source: c:\Photos\Backup\IMG_001.jpg (2.45 MB)
  Target: c:\Organized\2024\03\20240315_143022_IMG_001.jpg
```

### Scenario 2: Different File, Same Name
**Action:** Copy with numbered suffix (_1, _2, etc.)
```
[2024-03-15 14:30:25] [3/500] COPY (DUPLICATE - DIFFERENT HASH)
  Source: c:\Photos\Archive\IMG_001.jpg (2.50 MB)
  Original Target: c:\Organized\2024\03\20240315_143022_IMG_001.jpg
  Actual Target: c:\Organized\2024\03\20240315_143022_IMG_001_1.jpg
```

### Scenario 3: New File
**Action:** Copy normally
```
[2024-03-15 14:30:23] [1/500] COPY
  Source: c:\Photos\IMG_001.jpg (2.45 MB)
  Target: c:\Organized\2024\03\20240315_143022_IMG_001.jpg
```

## Log File Format

The copy operation generates a detailed log (`copy-log.txt`):

```
File Copy Operation Log - Started: 2024-03-15 14:30:22
====================================================================================================

[2024-03-15 14:30:23] [1/500] COPY
  Source: c:\Photos\IMG_001.jpg (2.45 MB)
  Target: c:\Organized\2024\03\20240315_143022_IMG_001.jpg

[2024-03-15 14:30:24] [2/500] SKIP (DUPLICATE - SAME HASH)
  Source: c:\Photos\Backup\IMG_001.jpg (2.45 MB)
  Target: c:\Organized\2024\03\20240315_143022_IMG_001.jpg

[2024-03-15 14:30:25] [3/500] COPY (DUPLICATE - DIFFERENT HASH)
  Source: c:\Photos\Archive\IMG_001.jpg (2.50 MB)
  Original Target: c:\Organized\2024\03\20240315_143022_IMG_001.jpg
  Actual Target: c:\Organized\2024\03\20240315_143022_IMG_001_1.jpg

====================================================================================================
Operation completed: 2024-03-15 14:35:30
Total files processed: 500
Files copied: 450
Files skipped: 50
```

### Log Entry Components

- **Timestamp:** `[2024-03-15 14:30:23]` - Date and time to the second
- **Progress:** `[1/500]` - Current file number / Total files
- **Operation Type:** `COPY`, `SKIP`, or `ERROR`
- **File Size:** Displayed in KB or MB
- **Paths:** Source and target file paths

## Performance Optimization

The application is optimized for speed:

1. **Size Check First:** Compares file sizes before computing hash
2. **Hash on Demand:** Only calculates SHA256 hash when files have the same size
3. **Progress Reporting:** Updates every second during processing
4. **Efficient I/O:** Streams file content for hash calculation

## Supported File Types

Default configuration includes:

**Images:**
- `.jpg`, `.jpeg`, `.png`, `.bmp`, `.gif`

**Videos:**
- `.mp4`, `.avi`, `.mov`, `.mpeg`, `.mpg`, `.wmv`, `.3gp`, `.vob`, `.ts`, `.mkv`

You can customize the list in `appsettings.json` by editing the `FileExtensions` key.

## Example Workflow

1. **Configure** `appsettings.json` with your source and target folders
2. **Run analysis:**
   ```bash
   dotnet run --analyze
   ```
3. **Review** `file-analysis.txt` to verify file counts
4. **Run copy operation:**
   ```bash
   dotnet run --copy
   ```
5. **Check** `copy-log.txt` for detailed operation results

## Building for Release

To create a self-contained executable:

```bash
dotnet publish -c Release -r win-x64 --self-contained
```

Replace `win-x64` with your target runtime:
- `win-x64` - Windows 64-bit
- `linux-x64` - Linux 64-bit
- `osx-x64` - macOS 64-bit

## Project Structure

```
FileOrganizer/
??? FileOrganizer.csproj    # Project file
??? Program.cs              # Main application logic
??? appsettings.json        # Configuration file
??? README.md               # This file
```

## Dependencies

- [Microsoft.Extensions.Configuration.Json](https://www.nuget.org/packages/Microsoft.Extensions.Configuration.Json/) - Configuration support
- [System.CommandLine](https://www.nuget.org/packages/System.CommandLine/) - Command-line argument parsing

## License

This project is open source and available under the [MIT License](LICENSE).

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Author

Created by [Your Name]

## Support

If you encounter any issues or have questions, please open an issue on GitHub.
