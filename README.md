# DoDo.Net Text Extraction Library

A powerful, extensible .NET library for extracting text from various file formats including PDF, Office documents, HTML, and more.

## Features

- **Multiple File Format Support**: PDF, Word (.docx), Excel (.xlsx, .xls), PowerPoint (.pptx), HTML, plain text, JSON, XML, CSV, Markdown, and more
- **Extensible Architecture**: Easily add support for new file formats without modifying existing code
- **AOT-Friendly**: No reflection usage, compatible with Native AOT compilation
- **Async/Parallel Processing**: High-performance text extraction with parallel file processing
- **Automatic Encoding Detection**: Intelligently detects file encoding for text-based files
- **Error Handling**: Comprehensive error handling with event-based error reporting
- **Directory Processing**: Recursive directory scanning with depth control and file filtering

## Supported File Extensions

- **Text Files**: .txt, .csv, .json, .xml, .md, .markdown, .yml, .yaml, .ini, .cfg, .config, .log, .tsv
- **HTML Files**: .html, .htm
- **PDF Files**: .pdf
- **Office Documents**: .docx (Word), .xlsx/.xls (Excel), .pptx (PowerPoint)

## Installation

Add the following NuGet packages to your project:

```xml
<PackageReference Include="itext7" Version="8.0.5" />
<PackageReference Include="DocumentFormat.OpenXml" Version="3.1.0" />
<PackageReference Include="EPPlus" Version="7.3.2" />
<PackageReference Include="HtmlAgilityPack" Version="1.11.65" />
<PackageReference Include="UDE.NetStandard" Version="1.2.0" />
```

## Usage

### Basic Usage

```csharp
using DoDo.Net.TextExtraction;

// Create the text extraction service
var textExtractor = new TextExtractionService();

// Extract text from specific files
var results = await textExtractor.ReadFromFilesAsync("document.pdf", "data.xlsx", "content.html");

foreach (var result in results)
{
    Console.WriteLine($"File: {result.FileName}");
    Console.WriteLine($"Text: {result.Text}");
    Console.WriteLine();
}
```

### Directory Processing

```csharp
// Extract from all supported files in a directory (recursive)
var directoryResults = await textExtractor.ReadFromDirectoryAsync(@"C:\Documents");

// Extract with depth control (max 2 levels deep)
var limitedResults = await textExtractor.ReadFromDirectoryAsync(@"C:\Documents", maxDepth: 2);

// Extract from multiple directories
var multiResults = await textExtractor.ReadFromDirectoriesAsync(new[] { @"C:\Docs1", @"C:\Docs2" });
```

### Error Handling

```csharp
// Subscribe to error events
textExtractor.ExtractionError += (sender, args) =>
{
    Console.WriteLine($"Error processing {args.FilePath}: {args.Message}");
    // Log the exception details if needed
    Console.WriteLine($"Exception: {args.Exception}");
};
```

### Adding Custom Extractors

```csharp
// Create a custom extractor for .log files
public class CustomLogExtractor : ITextExtractor
{
    public IReadOnlySet<string> SupportedExtensions { get; } = 
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".log" };

    public async Task<string> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var content = await File.ReadAllTextAsync(filePath, cancellationToken);
        
        // Custom processing logic here
        // For example, extract only error messages
        var lines = content.Split('\n');
        var errorMessages = lines
            .Where(line => line.Contains("ERROR"))
            .Select(line => line.Substring(line.IndexOf("ERROR")))
            .ToList();
            
        return string.Join("\n", errorMessages);
    }
}

// Register the custom extractor
textExtractor.RegisterExtractor(new CustomLogExtractor());
```

## API Reference

### TextExtractionService

The main service class for text extraction operations.

#### Methods

- `Task<IEnumerable<FileTextResult>> ReadFromFilesAsync(params string[] files)`
  - Extracts text from specific files
  - Processes files in parallel for better performance

- `Task<IEnumerable<FileTextResult>> ReadFromDirectoryAsync(string directory, int maxDepth = int.MaxValue, bool recursive = true)`
  - Extracts text from all supported files in a directory
  - Supports depth control and recursive scanning

- `Task<IEnumerable<FileTextResult>> ReadFromDirectoriesAsync(string[] directories, int maxDepth = int.MaxValue, bool recursive = true)`
  - Extracts text from multiple directories

- `void RegisterExtractor(ITextExtractor extractor)`
  - Registers a custom text extractor

#### Properties

- `IReadOnlySet<string> SupportedExtensions`
  - Gets all currently supported file extensions

#### Events

- `event EventHandler<TextExtractionErrorEventArgs> ExtractionError`
  - Raised when an error occurs during text extraction

### FileTextResult

Represents the result of text extraction from a file.

#### Properties

- `string FilePath` - The full path to the original file
- `string Text` - The extracted text content
- `string Extension` - The file extension
- `string FileName` - The file name without path

### ITextExtractor Interface

Interface for implementing custom text extractors.

#### Properties

- `IReadOnlySet<string> SupportedExtensions` - File extensions this extractor supports

#### Methods

- `Task<string> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default)`
  - Extracts text from a file

## Performance Considerations

- The library uses parallel processing for multiple files to maximize performance
- Large files are processed asynchronously to avoid blocking the UI thread
- Memory usage is optimized by processing files individually rather than loading all into memory at once
- Semaphore limits are used to prevent excessive concurrent file operations

## Error Handling

The library provides comprehensive error handling:

- File not found errors
- Unsupported file format errors
- File access permission errors
- Corrupted file errors
- Custom error event handling

If no error event handler is registered, errors are written to the console by default.

## License

This library uses several third-party components:
- iText7 for PDF processing (AGPL/Commercial license)
- EPPlus for Excel processing (Polyform Noncommercial license)
- DocumentFormat.OpenXml for Office documents (MIT license)
- HtmlAgilityPack for HTML processing (MIT license)
- UDE.NetStandard for encoding detection (MIT license)

Please ensure you comply with the respective licenses when using this library in your projects.

## Contributing

To add support for new file formats:

1. Create a new class implementing `ITextExtractor`
2. Specify the supported file extensions
3. Implement the `ExtractTextAsync` method
4. Register the extractor using `RegisterExtractor()`

The library is designed to be extensible without requiring changes to existing code, making it easy to add new file format support.
