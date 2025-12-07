# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- **Dry-run mode** - Preview operations without making changes (`--dry-run`)
- **Duplicate finder** - Find duplicate files in target folder (`--find-duplicates`)
- **Smart cleanup** - Remove duplicates keeping correctly timestamped files (`--clean-duplicates`)
- **Performance statistics** - Detailed stats after copy operations
  - File counts and percentages
  - Data sizes (copied, saved, total)
  - Time elapsed and processing speed
  - Average copy speed (MB/s)
- **Refactored architecture** - Split into multiple classes:
  - `FileAnalyzer` - File analysis functionality
  - `FileCopier` - File copying operations
  - `FileHashHelper` - SHA256 hash computation
  - `PathHelper` - Path manipulation and macros
  - `DuplicateFinder` - Duplicate detection
  - `DuplicateCleaner` - Duplicate removal
  - `AppConfiguration` - Configuration management
- **Universal DateTime macros** - Support for any .NET DateTime format string
- **Error tracking** - Count and display errors during operations
- **Duplicate detection log** - Detailed report of found duplicates with timestamp analysis
- **Cleanup summary** - Statistics about deleted files and freed space

### Changed
- Program.cs refactored to 40 lines (from 400+)
- Improved code organization with Single Responsibility Principle
- Enhanced console output with ASCII box formatting
- Updated to .NET 9 (latest stable version)

### Documentation
- Added DUPLICATE-DETECTION.md - Comprehensive duplicate management guide
- Updated README.md with new features and workflows
- Added complete usage examples
- Documented all configuration options

## [1.0.0] - 2024-12-07

### Added
- Initial release
- File analysis by extension with detailed reports
- Smart file copying with date-based folder organization
- SHA256 hash-based duplicate detection during copy
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
