using DoDo.Net;

// Create the text extraction service
var textExtractor = new TextExtractionService();

Console.WriteLine("=== DoDo.Net Text Extraction Library Demo ===");
Console.WriteLine();

// Display registered extractors
Console.WriteLine("Registered extractors:");
foreach (var extractor in textExtractor.RegisteredExtractors)
{
    Console.WriteLine($"  {extractor.GetType().Name}");
}
Console.WriteLine();

// Demo 1: Extract from specific files
Console.WriteLine("Demo 1: Extract text from specific files");
Console.WriteLine("----------------------------------------");

// Create some sample files for demonstration
await CreateSampleFiles();

var sampleFiles = new[]
{
    "sample.txt",
    "sample.html",
    "sample.json",
    "sample.md"
};

await foreach (var result in textExtractor.ReadFromFilesAsync(default, sampleFiles))
{
    Console.WriteLine($"File: {result.FilePath}");
    Console.WriteLine($"Text Preview: {TruncateText(result.Text, 100)}");
    Console.WriteLine();
}

// Demo 2: Extract from current directory (non-recursive)
Console.WriteLine("Demo 2: Extract from current directory (non-recursive, max depth 1)");
Console.WriteLine("--------------------------------------------------------------------");


await foreach (var result in textExtractor.ReadFromDirectoryAsync(".", maxDepth: 1, recursive: false))
{
    Console.WriteLine($"  {result.FilePath} - {result.Text.Length} characters");
}
Console.WriteLine();

// Demo 3: Custom extractor example
Console.WriteLine("Demo 3: Adding a custom extractor");
Console.WriteLine("----------------------------------");

// Register a custom extractor for .log files
textExtractor.RegisterExtractor(new CustomLogExtractor());

// Create a sample log file
await File.WriteAllTextAsync("sample.log", "[2024-01-01 10:00:00] INFO: Application started\n[2024-01-01 10:00:01] DEBUG: Loading configuration\n[2024-01-01 10:00:02] WARNING: Configuration file not found, using defaults\n[2024-01-01 10:00:03] ERROR: Failed to connect to database\n[2024-01-01 10:00:04] INFO: Ready to serve requests");

await foreach (var result in textExtractor.ReadFromFilesAsync(CancellationToken.None, "sample.log"))
{
    Console.WriteLine($"Custom Log Extractor Result:");
    Console.WriteLine(result.Text);
    break; // Only process the first (and only) result
}

Console.WriteLine();
Console.WriteLine("Demo completed successfully!");

// Helper method to create sample files
static async Task CreateSampleFiles()
{
    // Sample text file
    await File.WriteAllTextAsync("sample.txt", "This is a sample text file.\nIt contains multiple lines.\nEach line has different content.");
    
    // Sample HTML file
    await File.WriteAllTextAsync("sample.html", @"
<!DOCTYPE html>
<html>
<head><title>Sample HTML</title></head>
<body>
    <h1>Welcome to HTML Extraction</h1>
    <p>This is a paragraph with <strong>bold text</strong> and <em>italic text</em>.</p>
    <ul>
        <li>First item</li>
        <li>Second item</li>
    </ul>
</body>
</html>");
    
    // Sample JSON file
    await File.WriteAllTextAsync("sample.json", @"{
    ""name"": ""John Doe"",
    ""age"": 30,
    ""address"": {
        ""street"": ""123 Main St"",
        ""city"": ""Anytown"",
        ""zipcode"": ""12345""
    },
    ""hobbies"": [""reading"", ""swimming"", ""coding""]
}");
    
    // Sample Markdown file
    await File.WriteAllTextAsync("sample.md", @"# Sample Markdown Document

This is a **markdown** file with various formatting.

## Features
- Supports *italic* and **bold** text
- Code blocks: `var x = 10;`
- Lists and headers

### Code Example
```csharp
public class Example 
{
    public void Method() => Console.WriteLine(""Hello World"");
}
```

> This is a blockquote with important information.
");
}

static string TruncateText(string text, int maxLength)
{
    if (string.IsNullOrEmpty(text))
        return "<empty>";
        
    if (text.Length <= maxLength)
        return text;
        
    return text[..maxLength] + "...";
}

// Example of a custom extractor
public class CustomLogExtractor : ITextExtractor
{
    public bool IsSupported(string filePath)
    {
        return FileExtensionHelper.HasExtension(filePath, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".log" });
    }

    public async Task<string> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var content = await File.ReadAllTextAsync(filePath, cancellationToken);
        
        // Custom log processing - extract only ERROR and WARNING lines
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var importantLines = lines.Where(line => 
            line.Contains("ERROR", StringComparison.OrdinalIgnoreCase) || 
            line.Contains("WARNING", StringComparison.OrdinalIgnoreCase));
        
        return string.Join('\n', importantLines);
    }
}
