# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2024-03-15

### Added
- Initial release
- File analysis by extension with detailed reports
- Smart file copying with date-based folder organization
- SHA256 hash-based duplicate detection
- Intelligent collision handling with automatic numbering
- Detailed operation logging with timestamps
- Progress tracking (X/Total files)
- File size optimization (size check before hash calculation)
- Support for photos and videos (configurable extensions)
- Configurable target folder patterns with macros:
  - `{FileDate:yyyy}` - Year
  - `{FileDate:MM}` - Month
  - `{FileDate:yyyyMMdd_HHmmss}` - Full timestamp
  - `{FileName}` - Original file name
  - `{FileExtension}` - File extension
- Command-line options:
  - `--analyze` - Analyze files and generate report
  - `--copy` - Copy and organize files
- Three duplicate handling scenarios:
  - Skip identical files (same hash)
  - Copy with numbering for different files with same name
  - Normal copy for new files

### Performance
- Optimized hash calculation (only when necessary)
- Size comparison before expensive hash operations
- Progress updates every second during processing

### Documentation
- Comprehensive README.md
- Quick Start Guide
- MIT License
- Example configuration file
- GitHub Actions CI/CD workflow

## [Unreleased]

### Planned Features
- Parallel processing support for faster operations
- Move operation (not just copy)
- Dry-run mode
- File filtering by date range
- Custom date source (EXIF data for photos)
- Resume interrupted operations
- Statistics summary at the end
