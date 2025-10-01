using System.Runtime.CompilerServices;
using DoDo.Net.TextExtraction.Extractors;

namespace DoDo.Net.TextExtraction;


/// <summary>
/// Main service for extracting text from various file formats
/// </summary>
public class TextExtractionService : ITextExtractionService
{
    private readonly ExtractorRegistry _registry;
    
    /// <summary>
    /// Event raised when an error occurs during text extraction
    /// </summary>
    public event EventHandler<TextExtractionErrorEventArgs>? ExtractionError;
    
    public TextExtractionService()
    {
        _registry = new ExtractorRegistry();
        RegisterDefaultExtractors();
    }
    
    /// <summary>
    /// Gets all supported file extensions
    /// </summary>
    public IReadOnlySet<string> SupportedExtensions => _registry.SupportedExtensions;
    
    /// <summary>
    /// Registers a custom text extractor
    /// </summary>
    public void RegisterExtractor(ITextExtractor extractor)
    {
        _registry.RegisterExtractor(extractor);
    }
    
    /// <summary>
    /// Extracts text from multiple files
    /// </summary>
    public async IAsyncEnumerable<FileTextResult> ReadFromFilesAsync(string[] files, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var filePath in files)
        {
            var result = await ReadFromFileAsync(filePath, cancellationToken);
            yield return result;
        }
    }
    
    /// <summary>
    /// Extracts text from all supported files in a directory
    /// </summary>
    public async IAsyncEnumerable<FileTextResult> ReadFromDirectoryAsync(
        string directory, 
        int maxDepth = int.MaxValue, 
        bool recursive = true, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var files = GetSupportedFilesFromDirectory(directory, maxDepth, recursive);
        foreach (var filePath in files)
        {
            yield return await ReadFromFileAsync(filePath, cancellationToken);
        }
    }
    
    private void RegisterDefaultExtractors()
    {
        // Register all default extractors manually (AOT-friendly)
        _registry.RegisterExtractor(new PlainTextExtractor());
        _registry.RegisterExtractor(new HtmlExtractor());
        _registry.RegisterExtractor(new PdfExtractor());
        _registry.RegisterExtractor(new WordExtractor());
        _registry.RegisterExtractor(new ExcelExtractor());
        _registry.RegisterExtractor(new PowerPointExtractor());
    }
    
    public async Task<FileTextResult> ReadFromFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                OnExtractionError(filePath, new FileNotFoundException($"File not found: {filePath}"), 
                    $"File not found: {filePath}");
                return new FileTextResult{ FilePath = filePath, Success = false, ErrorMessage = "File not found" };
            }
            
            var extension = Path.GetExtension(filePath);
            if (!_registry.TryGetExtractor(extension, out var extractor))
            {
                OnExtractionError(filePath, new NotSupportedException($"Unsupported file extension: {extension}"), 
                    $"Unsupported file extension: {extension}");
                return new FileTextResult{ FilePath = filePath, Success = false, ErrorMessage = "Unsupported file extension" };
            }
            
            var text = await extractor!.ExtractTextAsync(filePath, cancellationToken);
            
            return new FileTextResult
            {
                FilePath = Path.GetFullPath(filePath),
                Text = text,
                Success = true
            };
        }
        catch (Exception ex)
        {
            OnExtractionError(filePath, ex, $"Error extracting text from {filePath}: {ex.Message}");
            return new FileTextResult{ FilePath = filePath, Success = false, ErrorMessage = ex.Message };
        }
    }
    
    private List<string> GetSupportedFilesFromDirectory(string directory, int maxDepth, bool recursive, int currentDepth = 0)
    {
        var files = new List<string>();
        
        try
        {
            // Get files in current directory
            var directoryFiles = Directory.GetFiles(directory)
                .Where(file => _registry.IsSupported(Path.GetExtension(file)))
                .ToList();
            
            files.AddRange(directoryFiles);
            
            // Recursively process subdirectories if enabled and within depth limit
            if (recursive && currentDepth < maxDepth)
            {
                var subdirectories = Directory.GetDirectories(directory);
                foreach (var subdirectory in subdirectories)
                {
                    var subFiles = GetSupportedFilesFromDirectory(subdirectory, maxDepth, recursive, currentDepth + 1);
                    files.AddRange(subFiles);
                }
            }
        }
        catch (Exception ex)
        {
            OnExtractionError(directory, ex, $"Error accessing directory {directory}: {ex.Message}");
        }
        
        return files;
    }
    
    private void OnExtractionError(string filePath, Exception exception, string message)
    {
        var args = new TextExtractionErrorEventArgs
        {
            FilePath = filePath,
            Exception = exception,
            Message = message
        };
        
        if (ExtractionError != null)
        {
            ExtractionError.Invoke(this, args);
        }
        else
        {
            // Default behavior: write to console
            Console.WriteLine($"[ERROR] {message}");
        }
    }
}
