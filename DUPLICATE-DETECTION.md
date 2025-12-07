# Duplicate Detection and Cleanup Guide

## Overview

This guide explains how to use the duplicate detection and cleanup features to find and remove duplicate files that may have been copied multiple times with different timestamps.

## Common Scenario

When organizing files, you might encounter:

```
Original file: Photo.jpg (taken on 2013-01-06 at 13:54:02)
```

After copying multiple times from different sources:

```
c:\Organized\2013\01\20130106_135402_Photo.jpg  (correct timestamp)
c:\Organized\2013\01\20130106_145402_Photo.jpg  (wrong timestamp)
c:\Organized\2013\01\20130106_155402_Photo.jpg  (wrong timestamp)
```

All three files have **identical content** but different timestamps in the filename. The duplicate finder detects this, and the cleaner removes the incorrectly timestamped copies.

## How It Works

### 1. Duplicate Detection

The `--find-duplicates` command:

1. **Scans** the target folder recursively
2. **Extracts** original filename from each file (removes `yyyyMMdd_HHmmss_` prefix)
3. **Groups** files by original name and file size
4. **Computes** SHA256 hash for files in the same group
5. **Identifies** duplicates (same hash, different timestamps)
6. **Analyzes** each duplicate:
   - Extracts date from filename (`20130106_135402` ? `2013-01-06 13:54:02`)
   - Compares with file's LastWriteTime
   - Marks as KEEP (match) or DELETE? (mismatch)

### 2. Duplicate Cleanup

The `--clean-duplicates` command:

1. **Reads** the duplicates log
2. **Checks** each file still exists
3. **Compares** filename timestamp vs actual file timestamp
4. **Keeps** files where timestamps match (±2 seconds tolerance)
5. **Deletes** files where timestamps don't match
6. **Safety**: Always keeps at least one file per group

## Step-by-Step Guide

### Step 1: Organize Your Files First

```bash
dotnet run --copy
```

This creates organized files like:
```
c:\Organized\2013\01\20130106_135402_Photo.jpg
c:\Organized\2024\03\20240315_143022_Document.pdf
```

### Step 2: Find Duplicates

```bash
dotnet run --find-duplicates
```

**Console Output:**
```
Searching for duplicates in: c:\Organized
Found 1500 files to analyze...
Analyzed: 1500/1500 files (100.0%)
Checking for duplicates by hash...

Duplicate detection complete!
Found 15 duplicate groups with 42 total files
Detailed log saved to: duplicates-log_20241207_150000.txt
```

### Step 3: Review the Log

Open `duplicates-log_20241207_150000.txt`:

```
Duplicate Files Report - 2024-12-07 15:00:00
====================================================================================================
Total duplicate groups found: 15
Total duplicate files: 42

=== Duplicate Group #1 ===
Original Name: Photo
File Size: 2.45 MB
Number of copies: 3

  Path: c:\Organized\2013\01\20130106_135402_Photo.jpg
  Date in filename: 2013-01-06 13:54:02
  Actual file date: 2013-01-06 13:54:02
  Match: YES (KEEP)

  Path: c:\Organized\2013\01\20130106_145402_Photo.jpg
  Date in filename: 2013-01-06 14:54:02
  Actual file date: 2013-01-06 13:54:02
  Match: NO (DELETE?)

  Path: c:\Organized\2013\01\20130106_155402_Photo.jpg
  Date in filename: 2013-01-06 15:54:02
  Actual file date: 2013-01-06 13:54:02
  Match: NO (DELETE?)

=== Duplicate Group #2 ===
Original Name: Document
File Size: 1.23 MB
Number of copies: 2

  Path: c:\Organized\2024\03\20240315_091500_Document.pdf
  Date in filename: 2024-03-15 09:15:00
  Actual file date: 2024-03-15 09:15:01
  Match: YES (KEEP)

  Path: c:\Organized\2024\03\20240315_143000_Document.pdf
  Date in filename: 2024-03-15 14:30:00
  Actual file date: 2024-03-15 09:15:01
  Match: NO (DELETE?)
```

### Step 4: Preview Cleanup (Dry Run)

```bash
dotnet run --clean-duplicates --dry-run
```

**Console Output:**
```
=== DRY RUN MODE - No files will be actually deleted ===

Reading duplicates from: duplicates-log_20241207_150000.txt
Found 15 duplicate groups
Processing...

Processing group: Photo
  KEEP: 20130106_135402_Photo.jpg (date matches)
  WOULD DELETE: 20130106_145402_Photo.jpg (date mismatch)
  WOULD DELETE: 20130106_155402_Photo.jpg (date mismatch)

Processing group: Document
  KEEP: 20240315_091500_Document.pdf (date matches)
  WOULD DELETE: 20240315_143000_Document.pdf (date mismatch)

========================================================
|                  CLEANUP SUMMARY                     |
========================================================
| Duplicate groups processed:         15              |
| Files would be deleted:             27              |
| Files kept:                         15              |
| Space would be freed:            15.23 GB           |
========================================================

This was a DRY RUN. No files were actually deleted.
Run without --dry-run to perform the actual cleanup.
```

### Step 5: Perform Actual Cleanup

```bash
dotnet run --clean-duplicates
```

**Console Output:**
```
Reading duplicates from: duplicates-log_20241207_150000.txt
Found 15 duplicate groups
Processing...

Processing group: Photo
  KEEP: 20130106_135402_Photo.jpg (date matches)
  DELETE: 20130106_145402_Photo.jpg (date mismatch)
  DELETE: 20130106_155402_Photo.jpg (date mismatch)

Processing group: Document
  KEEP: 20240315_091500_Document.pdf (date matches)
  DELETE: 20240315_143000_Document.pdf (date mismatch)

========================================================
|                  CLEANUP SUMMARY                     |
========================================================
| Duplicate groups processed:         15              |
| Files deleted:                      27              |
| Files kept:                         15              |
| Space freed:                     15.23 GB           |
========================================================
```

## Understanding the Logic

### Timestamp Matching

The cleaner compares:
- **Filename timestamp**: Extracted from `yyyyMMdd_HHmmss` prefix
- **File timestamp**: File's LastWriteTime property

**Match criteria:**
- Difference must be ? 2 seconds (tolerance for file system precision)
- If match ? **KEEP** the file (correct timestamp)
- If no match ? **DELETE** the file (incorrect timestamp)

### Safety Measures

1. **Always keep at least one file** per duplicate group
2. **Dry-run mode** available for preview
3. **Detailed logging** of every decision
4. **File existence check** before deletion
5. **Error handling** for deletion failures

## Example Scenarios

### Scenario 1: Perfect Match

```
File: 20130106_135402_Photo.jpg
Filename date: 2013-01-06 13:54:02
File date:     2013-01-06 13:54:02
Difference:    0 seconds
Result:        KEEP ?
```

### Scenario 2: Close Match (within tolerance)

```
File: 20130106_135402_Photo.jpg
Filename date: 2013-01-06 13:54:02
File date:     2013-01-06 13:54:01
Difference:    1 second
Result:        KEEP ? (within 2-second tolerance)
```

### Scenario 3: Mismatch

```
File: 20130106_145402_Photo.jpg
Filename date: 2013-01-06 14:54:02
File date:     2013-01-06 13:54:02
Difference:    3600 seconds (1 hour)
Result:        DELETE ?
```

### Scenario 4: All Files Mismatch (Safety)

```
Group has 3 files, all with mismatched timestamps
Action: Keep the first file anyway (safety measure)
Result: Delete only 2 files, keep 1
```

## Configuration

Add to `appsettings.json`:

```json
{
  "DuplicatesLogFileName": "duplicates-log_{DateTime:yyyyMMdd_HHmmss}.txt"
}
```

This filename supports DateTime macros just like other log files, preventing overwrites.

## Tips and Best Practices

1. **Always run `--find-duplicates` before `--clean-duplicates`**
   - The cleanup reads the log generated by find-duplicates

2. **Use dry-run first**
   - Preview what will be deleted: `--clean-duplicates --dry-run`
   - Review the output carefully

3. **Check the duplicates log manually**
   - Open the log file and verify the KEEP/DELETE decisions
   - Look for any unexpected patterns

4. **Backup important files first**
   - The cleanup is permanent
   - Consider backing up the target folder before cleanup

5. **Run find-duplicates periodically**
   - After adding more files to organized folder
   - To catch newly introduced duplicates

6. **Verify freed space**
   - Check the cleanup summary
   - Compare folder size before and after

## Troubleshooting

### "Duplicates log file does not exist"
- Run `--find-duplicates` first to generate the log
- Check the log filename in appsettings.json

### "No duplicates found"
- This is good! Your organized folder has no duplicates
- Or files don't follow the `yyyyMMdd_HHmmss_Name` pattern

### "Too many files marked for deletion"
- Review the duplicates log carefully
- Check if LastWriteTime is preserved during copy
- Consider adjusting timestamp tolerance (requires code change)

### Files not deleted during cleanup
- Check file permissions
- Look for error messages in console output
- File might be in use by another program

## Advanced: Custom Patterns

The duplicate finder expects filenames in this format:

```
yyyyMMdd_HHmmss_OriginalName.ext
```

Where:
- `yyyyMMdd` = Year Month Day (8 digits)
- `HHmmss` = Hour Minute Second (6 digits)
- `OriginalName` = The original filename
- `.ext` = File extension

If your files use a different pattern, you'll need to modify the code in `DuplicateFinder.cs` and `DuplicateCleaner.cs`.
