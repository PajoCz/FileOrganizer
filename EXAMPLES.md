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
??? 2024\
?   ??? 01\
?   ?   ??? 15\
?   ?   ?   ??? IMG_001.jpg
?   ?   ?   ??? IMG_002.jpg
?   ?   ??? 20\
?   ?       ??? photo.png
?   ??? 03\
?       ??? 10\
?           ??? family.jpg
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
??? 2023\
?   ??? 12-December\
?   ?   ??? birthday.mp4
?   ?   ??? christmas.mov
??? 2024\
    ??? 01-January\
    ?   ??? newyear.mp4
    ??? 03-March\
        ??? vacation.avi
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
??? 2024-03-15_140522_IMG_001.jpg
??? 2024-03-15_140523_IMG_002.raw
??? 2024-03-15_143012_photo.cr2
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
??? 2024\
    ??? Event_20240315\
    ?   ??? 140522_photo1.jpg
    ?   ??? 140530_photo2.jpg
    ?   ??? 150100_video.mp4
    ??? Event_20240320\
        ??? 100000_meeting.mp4
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
??? 2022\
?   ??? vacation_001.jpg
?   ??? vacation_002.jpg
??? 2023\
?   ??? birthday.jpg
?   ??? wedding.jpg
??? 2024\
    ??? newphoto.jpg
```

## Handling Duplicates

### Scenario 1: Exact Duplicate
```
Source: c:\Photos\IMG_001.jpg (5 MB, hash: abc123...)
Target: Already exists with same hash

Result: SKIP (DUPLICATE - SAME HASH)
Log: [2024-03-15 14:30:24] [2/500] SKIP (DUPLICATE - SAME HASH)
     Source: c:\Photos\IMG_001.jpg (5.00 MB)
     Target: c:\Organized\2024\03\IMG_001.jpg
```

### Scenario 2: Different File, Same Name
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

## Command Examples

```bash
# Analyze files first
dotnet run --analyze

# Review analysis
cat file-analysis.txt

# Copy files
dotnet run --copy

# Review results
cat copy-log.txt

# Build for release
dotnet publish -c Release -r win-x64 --self-contained

# Run the published executable
.\bin\Release\net8.0\win-x64\publish\FileOrganizer.exe --copy
```
