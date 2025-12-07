# Quick Start Guide

## First Time Setup

1. **Clone the repository:**
   ```bash
   git clone https://github.com/yourusername/FileOrganizer.git
   cd FileOrganizer
   ```

2. **Create your configuration file:**
   ```bash
   cd FileOrganizer
   cp appsettings.json.example appsettings.json
   ```

3. **Edit `appsettings.json`** with your actual paths:
   - Set `SourceFolder` to your photos/videos location
   - Set `TargetFolderPattern` to where you want organized files
   
   Example:
   ```json
   {
     "SourceFolder": "c:\\Users\\YourName\\Pictures",
     "TargetFolderPattern": "c:\\Users\\YourName\\Organized\\{FileDate:yyyy}\\{FileDate:MM}\\{FileDate:yyyyMMdd_HHmmss}_{FileName}.{FileExtension}"
   }
   ```

4. **Build the project:**
   ```bash
   dotnet build
   ```

5. **Run analysis first (recommended):**
   ```bash
   dotnet run --analyze
   ```
   
   Review the generated `file-analysis.txt` to see what will be processed.

6. **Copy files:**
   ```bash
   dotnet run --copy
   ```
   
   Check `copy-log.txt` for detailed results.

## Common Issues

### "SourceFolder not configured"
Make sure you created `appsettings.json` from the example file and edited the paths.

### "Source folder does not exist"
Verify the path in `SourceFolder` exists and is accessible.

### Permission Denied
Run the application with appropriate permissions to read source and write to target folders.

## Tips

- **Always run `--analyze` first** to see what will be processed
- **Test with a small folder** before processing large collections
- **Check the log file** to verify all files were processed correctly
- **Backup important files** before organizing large collections
