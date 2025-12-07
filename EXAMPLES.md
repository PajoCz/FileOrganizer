# Usage Examples

## Example 1: Organize Family Photos

**Configuration:**
```json
{
  "SourceFolder": "c:\\Users\\John\\Downloads",
  "FileExtensions": ".jpg;.jpeg;.png;.heic",
  "TargetFolderPattern": "c:\\Users\\John\\Photos\\{FileDate:yyyy}\\{FileDate:MM}\\{FileDate:dd}\\{FileName}.{FileExtension}"
}
```

**Result:**
```
c:\Users\John\Photos\
- 2024\
  - 01\
    - 15\
      - IMG_001.jpg
      - IMG_002.jpg
    - 20\
      - photo.png
  - 03\
    - 10\
      - family.jpg
```

## Example 2: Archive Videos by Year and Month

**Configuration:**
```json
{
  "SourceFolder": "d:\\VideoBackup",
  "FileExtensions": ".mp4;.mov;.avi",
  "TargetFolderPattern": "e:\\Archive\\Videos\\{FileDate:yyyy}\\{FileDate:MM}-{FileDate:MMMM}\\{FileName}.{FileExtension}"
}
```

**Result:**
```
e:\Archive\Videos\
- 2023\
  - 12-December\
    - birthday.mp4
    - christmas.mov
- 2024\
  - 01-January\
    - newyear.mp4
  - 03-March\
    - vacation.avi
```

## Example 3: Keep Original Structure with Timestamp

**Configuration:**
```json
{
  "SourceFolder": "c:\\Camera",
  "FileExtensions": ".jpg;.raw;.cr2",
  "TargetFolderPattern": "c:\\Organized\\{FileDate:yyyy-MM-dd_HHmmss}_{FileName}.{FileExtension}"
}
```

**Result:**
```
c:\Organized\
- 2024-03-15_140522_IMG_001.jpg
- 2024-03-15_140523_IMG_002.raw
- 2024-03-15_143012_photo.cr2
```

## Example 4: Organize by Event Date

**Configuration:**
```json
{
  "SourceFolder": "c:\\Events",
  "FileExtensions": ".jpg;.png;.mp4",
  "TargetFolderPattern": "c:\\EventArchive\\{FileDate:yyyy}\\Event_{FileDate:yyyyMMdd}\\{FileDate:HHmmss}_{FileName}.{FileExtension}"
}
```

**Result:**
```
c:\EventArchive\
- 2024\
  - Event_20240315\
    - 140522_photo1.jpg
    - 140530_photo2.jpg
    - 150100_video.mp4
  - Event_20240320\
    - 100000_meeting.mp4
```

## Example 5: Simple Year-Based Archive

**Configuration:**
```json
{
  "SourceFolder": "c:\\AllPhotos",
  "FileExtensions": ".jpg;.jpeg;.png",
  "TargetFolderPattern": "c:\\PhotoArchive\\{FileDate:yyyy}\\{FileName}.{FileExtension}"
}
```

**Result:**
```
c:\PhotoArchive\
- 2022\
  - vacation_001.jpg
  - vacation_002.jpg
- 2023\
  - birthday.jpg
  - wedding.jpg
- 2024\
  - newphoto.jpg
```

## Example 6: Find and Clean Duplicates

**Scenario:** After organizing photos from multiple sources, you have duplicates with different timestamps.

### Step 1: Organize Files

```bash
dotnet run --copy
```

**Result:**
```
c:\Organized\2013\01\
- 20130106_135402_Photo.jpg  (copied from Camera backup)
- 20130106_145402_Photo.jpg  (copied from Phone backup - same photo!)
- 20130106_155402_Photo.jpg  (copied from Email backup - same photo!)
```

All three files are **identical** (same content) but have different timestamps in the filename.

### Step 2: Find Duplicates

```bash
dotnet run --find-duplicates
```

**Log Output (`duplicates-log.txt`):**
```
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
```

### Step 3: Preview Cleanup

```bash
dotnet run --clean-duplicates --dry-run
```

**Console Output:**
```
Processing group: Photo
  KEEP: 20130106_135402_Photo.jpg (date matches)
  WOULD DELETE: 20130106_145402_Photo.jpg (date mismatch)
  WOULD DELETE: 20130106_155402_Photo.jpg (date mismatch)

========================================================
|                  CLEANUP SUMMARY                     |
========================================================
| Duplicate groups processed:          1              |
| Files would be deleted:              2              |
| Files kept:                          1              |
| Space would be freed:             4.90 MB           |
========================================================
```

### Step 4: Clean Duplicates

```bash
dotnet run --clean-duplicates
```

**Final Result:**
```
c:\Organized\2013\01\
- 20130106_135402_Photo.jpg  (KEPT - correct timestamp)
```

**Freed:** 4.90 MB

## Handling Duplicates

### Scenario 1: Exact Duplicate (During Copy)
```
Source: c:\Photos\IMG_001.jpg (5 MB, hash: abc123...)
Target: Already exists with same hash

Result: SKIP (DUPLICATE - SAME HASH)
Log: [2024-03-15 14:30:24] [2/500] SKIP (DUPLICATE - SAME HASH)
     Source: c:\Photos\IMG_001.jpg (5.00 MB)
     Target: c:\Organized\2024\03\IMG_001.jpg
```

### Scenario 2: Different File, Same Name (During Copy)
```
Source: c:\Photos\IMG_001.jpg (5 MB, hash: xyz789...)
Target: Already exists with different hash (abc123...)

Result: COPY with number suffix
Log: [2024-03-15 14:30:25] [3/500] COPY (DUPLICATE - DIFFERENT HASH)
     Source: c:\Photos\IMG_001.jpg (5.00 MB)
     Original Target: c:\Organized\2024\03\IMG_001.jpg
     Actual Target: c:\Organized\2024\03\IMG_001_1.jpg
```

### Scenario 3: Multiple Different Files
```
IMG_001.jpg (hash: aaa) -> IMG_001.jpg
IMG_001.jpg (hash: bbb) -> IMG_001_1.jpg
IMG_001.jpg (hash: ccc) -> IMG_001_2.jpg
IMG_001.jpg (hash: bbb) -> SKIP (same as _1)
```

### Scenario 4: Duplicate Files in Target (After Organization)
```
Same file copied multiple times with different timestamps:
20130106_135402_Photo.jpg (hash: aaa, file date: 2013-01-06 13:54:02) -> KEEP
20130106_145402_Photo.jpg (hash: aaa, file date: 2013-01-06 13:54:02) -> DELETE
20130106_155402_Photo.jpg (hash: aaa, file date: 2013-01-06 13:54:02) -> DELETE
```

## Command Examples

```bash
# Analyze files first
dotnet run --analyze

# Review analysis
cat file-analysis.txt

# Preview copy operation
dotnet run --copy --dry-run

# Copy files
dotnet run --copy

# Review copy results
cat copy-log.txt

# Find duplicates in organized folder
dotnet run --find-duplicates

# Review duplicates
cat duplicates-log.txt

# Preview cleanup
dotnet run --clean-duplicates --dry-run

# Clean duplicates
dotnet run --clean-duplicates

# Build for release
dotnet publish -c Release -r win-x64 --self-contained

# Run the published executable
.\bin\Release\net9.0\win-x64\publish\FileOrganizer.exe --copy
```

## Complete Workflow Example

```bash
# 1. Initial setup
cd FileOrganizer
notepad appsettings.json  # Configure paths

# 2. Analyze source folder
dotnet run --analyze
# Output: file-analysis_20241207_100000.txt
# Found: 500 JPG files, 200 MP4 files

# 3. Preview copy
dotnet run --copy --dry-run
# Preview: Would copy 700 files

# 4. Actual copy
dotnet run --copy
# Result: 650 copied, 50 skipped (duplicates)
# Output: copy-log_20241207_100530.txt
# Statistics: Copied 15.2 GB in 5m 23s at 48 MB/s

# 5. Find duplicates in organized folder
dotnet run --find-duplicates
# Found: 25 duplicate groups, 75 total duplicates
# Output: duplicates-log_20241207_101015.txt

# 6. Review duplicates manually
notepad duplicates-log_20241207_101015.txt

# 7. Preview cleanup
dotnet run --clean-duplicates --dry-run
# Would delete: 50 files
# Would free: 12.5 GB

# 8. Clean duplicates
dotnet run --clean-duplicates
# Deleted: 50 files
# Freed: 12.5 GB
# Kept: 25 files (one per group)

# Final result: 600 unique files, 2.7 GB saved
