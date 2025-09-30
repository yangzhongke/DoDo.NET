using DoDo.Net.TextExtraction;

// Create the text extraction service
var textExtractor = new TextExtractionService();

// Subscribe to error events (optional)
textExtractor.ExtractionError += (sender, args) =>
{
    Console.WriteLine($"[Custom Error Handler] {args.Message}");
    Console.WriteLine($"File: {args.FilePath}");
    Console.WriteLine($"Exception: {args.Exception.GetType().Name}");
    Console.WriteLine();
};

Console.WriteLine("=== DoDo.Net Text Extraction Library Demo ===");
Console.WriteLine();

// Display supported file extensions
Console.WriteLine("Supported file extensions:");
foreach (var extension in textExtractor.SupportedExtensions.OrderBy(x => x))
{
    Console.WriteLine($"  {extension}");
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

await foreach (var result in textExtractor.ReadFromFilesAsync(sampleFiles))
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
await File.WriteAllTextAsync("sample.log", "[2024-01-01 10:00:00] INFO: Application started\n[2024-01-01 10:00:01] DEBUG: Loading configuration\n[2024-01-01 10:00:02] INFO: Ready to serve requests");

var logResult = await textExtractor.ExtractFromSingleFileAsync("sample.log");
Console.WriteLine(logResult.Text);

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
    public IReadOnlySet<string> SupportedExtensions { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".log"
    };

    public async Task<string> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var content = await File.ReadAllTextAsync(filePath, cancellationToken);
        
        // Custom processing: Extract only the message part from log entries
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var messages = new List<string>();
        
        foreach (var line in lines)
        {
            // Simple log parsing: extract everything after the log level
            var parts = line.Split("] ", 2);
            if (parts.Length == 2)
            {
                messages.Add(parts[1]);
            }
        }
        
        return string.Join("\n", messages);
    }
}
