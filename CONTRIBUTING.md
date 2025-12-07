# Contributing to File Organizer

Thank you for your interest in contributing to File Organizer! This document provides guidelines for contributing to the project.

## How to Contribute

### Reporting Bugs

If you find a bug, please create an issue with:
- Clear description of the problem
- Steps to reproduce
- Expected behavior
- Actual behavior
- Your environment (.NET version, OS)
- Log file snippets if applicable

### Suggesting Features

Feature suggestions are welcome! Please create an issue with:
- Clear description of the feature
- Use case explanation
- How it would benefit users
- Any implementation ideas (optional)

### Pull Requests

1. **Fork the repository**
   ```bash
   git clone https://github.com/yourusername/FileOrganizer.git
   cd FileOrganizer
   ```

2. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Make your changes**
   - Follow the existing code style
   - Keep changes focused and atomic
   - Add comments for complex logic
   - Update documentation if needed

4. **Test your changes**
   ```bash
   dotnet build
   dotnet run --analyze
   dotnet run --copy
   ```

5. **Commit your changes**
   ```bash
   git add .
   git commit -m "Add feature: your feature description"
   ```

6. **Push to your fork**
   ```bash
   git push origin feature/your-feature-name
   ```

7. **Create a Pull Request**
   - Describe your changes clearly
   - Reference any related issues
   - Explain testing performed

## Code Style Guidelines

- Use C# naming conventions
- Keep methods focused and small
- Add XML comments for public methods
- Use meaningful variable names
- Follow existing patterns in the codebase

## Commit Message Guidelines

- Use present tense ("Add feature" not "Added feature")
- Use imperative mood ("Move cursor to..." not "Moves cursor to...")
- First line should be concise (50 chars or less)
- Add detailed explanation in body if needed

Examples:
```
Add support for EXIF date extraction
Fix hash calculation for large files
Update README with new configuration options
```

## Development Setup

1. Install \.NET 10 SDK
2. Clone the repository
3. Open in your favorite IDE (Visual Studio, VS Code, Rider)
4. Create `appsettings.json` from example
5. Build and test

## Questions?

Feel free to create an issue for any questions about contributing!

