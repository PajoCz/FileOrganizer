# File Organizer

[![.NET Build](https://github.com/PajoCz/FileOrganizer/actions/workflows/dotnet.yml/badge.svg)](https://github.com/PajoCz/FileOrganizer/actions/workflows/dotnet.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET Version](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/download/dotnet/10.0)

A .NET 10 console application for analyzing and organizing files (photos and videos) by date with intelligent duplicate detection using SHA256 hashing.

## Features

- ✅ **File Analysis** - Analyze files by extension and generate detailed reports
- ✅ **Smart File Organization** - Copy files to date-based folder structure  
- ✅ **Duplicate Detection** - SHA256 hash-based duplicate detection during copy
- ✅ **Collision Handling** - Automatic numbering for files with same name but different content
- ✅ **Duplicate Finder** - Find duplicate files in target folder (same content, different timestamps)
- ✅ **Smart Cleanup** - Remove duplicates keeping only correctly timestamped files
- ✅ **Dry-Run Mode** - Preview operations before executing them
- ✅ **Detailed Logging** - Complete operation log with timestamps and file sizes
- ✅ **Performance Statistics** - Speed metrics, data sizes, and completion summaries
- ✅ **Performance Optimized** - Hash calculation only when necessary

## Requirements

- .NET 10.0 SDK or later
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
  "CopyLogFileName": "copy-log_{DateTime:yyyyMMdd_HHmms}.txt",
  "DuplicatesLogFileName": "duplicates-log_{DateTime:yyyyMMdd_HHmmss}.txt"
}
```

### Configuration Keys

| Key | Description | Example |
|-----|-------------|---------|
| `SourceFolder` | Source directory containing files to process | `c:\\Photos` |
| `OutputFileName` | Output file name for analysis report (supports DateTime macros) | `file-analysis_{DateTime:yyyyMMdd_HHmms}.txt` |
| `FileExtensions` | Semicolon-separated list of file extensions to process | `.jpg;.png;.mp4` |
| `TargetFolderPattern` | Target path pattern with macros (see below) | `c:\\Organized\\{FileDate:yyyy}\\{FileDate:MM}\\...` |
| `CopyLogFileName` | Log file name for copy operations (supports DateTime macros) | `copy-log_{DateTime:yyyyMMdd_HHmms}.txt` |
| `DuplicatesLogFileName` | Log file name for duplicate detection (supports DateTime macros) | `duplicates-log_{DateTime:yyyyMMdd_HHmms}.txt` |

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
- `{DateTime:yyyy-MM-dd_HH-mm-ss}` → `2024-03-15_14-30-22`
- `{DateTime:yyyyMMdd}` → `20240315`
- `{DateTime:HH-mm}` → `14-30`


**Example Configuration:**
```json
{
  "OutputFileName": "file-analysis_{DateTime:yyyyMMdd_HHmms}.txt",
  "CopyLogFileName": "copy-log_{DateTime:yyyyMMdd_HHmms}.txt"
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
c:\Organized\{FileDate:yyyy}\{FileDate:MM}\{FileDate:yyyyMMdd_HHmms}_{FileName}.{FileExtension}
```

**Results in:**
```
c:\Organized\2024\03\20240315_143022_IMG_001.jpg
```

## Usage

### Available Commands

```bash
dotnet run --analyze              # Analyze files in source folder
dotnet run --copy                 # Copy and organize files
dotnet run --copy --dry-run       # Preview copy operation
dotnet run --find-duplicates      # Find duplicate files in target folder
dotnet run --clean-duplicates     # Remove duplicates (keeps correct timestamps)
dotnet run --clean-duplicates --dry-run  # Preview cleanup
```

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

### Step 2: Preview Copy Operation (Optional)

Preview what will happen using dry-run mode:

```bash
dotnet run --copy --dry-run
```

This shows what would be copied without actually copying files.

### Step 3: Copy Files

After reviewing the analysis, copy files to the organized structure:

```bash
dotnet run --copy
```

This will:
1. Copy files matching configured extensions
2. Organize them using the date-based pattern
3. Detect and handle duplicates during copy
4. Generate detailed log file with statistics

### Step 4: Find Duplicates in Target Folder

After organizing files, you may want to find duplicates that were copied multiple times:

```bash
dotnet run --find-duplicates
```

This will:
1. Scan the target folder for files
2. Group files by original name and size
3. Compare file contents using SHA256 hash
4. Identify duplicates with different timestamps in filename
5. Generate a detailed duplicates log

**Example scenario:**
- Original file: `Photo.jpg`
- Copied as: `20130106_135402_Photo.jpg`
- Later copied again as: `20130106_145402_Photo.jpg`
- If both have the same content → Duplicate detected!

**Duplicate Log Example:**
```
=== Duplicate Group #1 ===
Original Name: Photo
File Size: 2.45 MB
Number of copies: 2

  Path: c:\Organized\2013\01\20130106_135402_Photo.jpg
  Date in filename: 2013-01-06 13:54:02
  Actual file date: 2013-01-06 13:54:02
  Match: YES (KEEP)

  Path: c:\Organized\2013\01\20130106_145402_Photo.jpg
  Date in filename: 2013-01-06 14:54:02
  Actual file date: 2013-01-06 13:54:02
  Match: NO (DELETE?)
```

### Step 5: Clean Duplicates

Remove duplicates keeping only files with matching timestamps:

```bash
# Preview cleanup first
dotnet run --clean-duplicates --dry-run

# Actual cleanup
dotnet run --clean-duplicates
```

This will:
1. Read the duplicates log from Step 4
2. Check each duplicate file
3. **Keep** files where filename timestamp matches file's LastWriteTime
4. **Delete** files where timestamps don't match
5. Always keep at least one file per group (safety measure)

**Cleanup Summary:**
```
========================================================
|                  CLEANUP SUMMARY                     |
========================================================
| Duplicate groups processed:         15              |
| Files deleted:                       27              |
| Files kept:                          15              |
| Space freed:                     15.23 GB            |
========================================================
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
  Actual Target: c:\Organized\2024\03\20240315_IMG_001_1.jpg

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

### Basic Workflow: Organize Files

1. **Configure** `appsettings.json` with your source and target folders
2. **Run analysis:**
   ```bash
   dotnet run --analyze
   ```
3. **Review** `file-analysis.txt` to verify file counts
4. **Preview copy (optional):**
   ```bash
   dotnet run --copy --dry-run
   ```
5. **Run copy operation:**
   ```bash
   dotnet run --copy
   ```
6. **Check** `copy-log.txt` for detailed operation results

### Advanced Workflow: Find and Clean Duplicates

1. **Organize files first** (steps 1-5 above)
2. **Find duplicates in target folder:**
   ```bash
   dotnet run --find-duplicates
   ```
3. **Review** `duplicates-log.txt` to see detected duplicates
4. **Preview cleanup:**
   ```bash
   dotnet run --clean-duplicates --dry-run
   ```
5. **Clean duplicates:**
   ```bash
   dotnet run --clean-duplicates
   ```
6. **Verify** freed space and kept files

### Complete Example

```bash
# 1. Analyze source folder
dotnet run --analyze
# Output: file-analysis_20241207_143022.txt

# 2. Copy and organize files
dotnet run --copy
# Output: copy-log_20241207_143530.txt
# Statistics: 450 files copied, 50 skipped

# 3. Find duplicates in organized folder
dotnet run --find-duplicates
# Output: duplicates-log_20241207_144015.txt
# Found: 15 duplicate groups

# 4. Preview duplicate cleanup
dotnet run --clean-duplicates --dry-run
# Preview: Would delete 27 files, keep 15 files, free 15.23 GB

# 5. Clean duplicates
dotnet run --clean-duplicates
# Result: Deleted 27 duplicates, freed 15.23 GB
```

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
→ FileOrganizer.csproj    # Project file
→ Program.cs              # Main application logic
→ appsettings.json        # Configuration file
→ README.md               # This file
```

## Dependencies

- [Microsoft.Extensions.Configuration.Json](https://www.nuget.org/packages/Microsoft.Extensions.Configuration.Json/) - Configuration support
- [System.CommandLine](https://www.nuget.org/packages/System.CommandLine/) - Command-line argument parsing

## License

This project is open source and available under the [MIT License](LICENSE).

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Author

Created by PajoCz

## Support

If you encounter any issues or have questions, please open an issue on GitHub.

















