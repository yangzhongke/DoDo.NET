using System.Runtime.CompilerServices;
using DoDo.Net.Extractors;

namespace DoDo.Net;

/// <summary>
/// Main service for extracting text from various file formats
/// </summary>
public class TextExtractionService
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
    /// Gets all registered extractors
    /// </summary>
    public IEnumerable<ITextExtractor> RegisteredExtractors => _registry.GetAllExtractors();
    
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
    public async IAsyncEnumerable<FileTextResult> ReadFromFilesAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default,
        params string[] files)
    {
        var semaphore = new SemaphoreSlim(Environment.ProcessorCount);
        var tasks = new List<Task<FileTextResult?>>();
        
        // Start all tasks
        foreach (var filePath in files)
        {
            var task = ProcessFileAsync(filePath, semaphore, cancellationToken);
            tasks.Add(task);
        }
        
        // Yield results as they complete
        while (tasks.Count > 0)
        {
            var completedTask = await Task.WhenAny(tasks);
            tasks.Remove(completedTask);
            
            var result = await completedTask;
            if (result != null)
            {
                yield return result;
            }
        }
        
        semaphore.Dispose();
    }
    
    /// <summary>
    /// Extracts text from all supported files in a directory
    /// </summary>
    public IAsyncEnumerable<FileTextResult> ReadFromDirectoryAsync(
        string directory, 
        int maxDepth = int.MaxValue, 
        bool recursive = true,
        CancellationToken cancellationToken = default)
    {
        return ReadFromDirectoriesAsync(new[] { directory }, maxDepth, recursive, cancellationToken);
    }
    
    /// <summary>
    /// Extracts text from all supported files in multiple directories
    /// </summary>
    public async IAsyncEnumerable<FileTextResult> ReadFromDirectoriesAsync(
        string[] directories, 
        int maxDepth = int.MaxValue, 
        bool recursive = true,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var semaphore = new SemaphoreSlim(Environment.ProcessorCount);
        var activeTasks = new List<Task<FileTextResult?>>();
        const int maxConcurrentTasks = 50; // Limit concurrent tasks to avoid overwhelming the system
        
        try
        {
            foreach (var directory in directories)
            {
                if (!Directory.Exists(directory))
                {
                    OnExtractionError(directory, new DirectoryNotFoundException($"Directory not found: {directory}"), 
                        $"Directory not found: {directory}");
                    continue;
                }
                
                await foreach (var filePath in GetSupportedFilesFromDirectoryAsync(directory, maxDepth, recursive, cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    // Start processing the file
                    var task = ProcessFileAsync(filePath, semaphore, cancellationToken);
                    activeTasks.Add(task);
                    
                    // If we have too many concurrent tasks, wait for some to complete
                    if (activeTasks.Count >= maxConcurrentTasks)
                    {
                        var completedTask = await Task.WhenAny(activeTasks);
                        activeTasks.Remove(completedTask);
                        
                        var result = await completedTask;
                        if (result != null)
                        {
                            yield return result;
                        }
                    }
                }
            }
            
            // Process remaining tasks
            while (activeTasks.Count > 0)
            {
                var completedTask = await Task.WhenAny(activeTasks);
                activeTasks.Remove(completedTask);
                
                var result = await completedTask;
                if (result != null)
                {
                    yield return result;
                }
            }
        }
        finally
        {
            semaphore.Dispose();
        }
    }
    
    private void RegisterDefaultExtractors()
    {
        // Register PlainTextExtractor first so it acts as fallback for files under 1MB
        _registry.RegisterExtractor(new FallbackPlainTextExtractor());
        
        // Register specific extractors first, PlainTextExtractor last as fallback
        // This order ensures newly registered extractors override older ones (forward search)
        _registry.RegisterExtractor(new HtmlExtractor());
        _registry.RegisterExtractor(new PdfExtractor());
        _registry.RegisterExtractor(new WordExtractor());
        _registry.RegisterExtractor(new ExcelExtractor());
        _registry.RegisterExtractor(new PowerPointExtractor());
        _registry.RegisterExtractor(new PlainTextExtractor());
    }
    
    private async Task<FileTextResult?> ProcessFileAsync(string filePath, SemaphoreSlim semaphore, CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            return await ExtractFromSingleFileAsync(filePath);
        }
        finally
        {
            semaphore.Release();
        }
    }
    
    private async Task<FileTextResult?> ExtractFromSingleFileAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                OnExtractionError(filePath, new FileNotFoundException($"File not found: {filePath}"), 
                    $"File not found: {filePath}");
                return null;
            }
            
            if (!_registry.TryGetExtractor(filePath, out var extractor))
            {
                OnExtractionError(filePath, new NotSupportedException($"No extractor available for file: {filePath}"), 
                    $"No extractor available for file: {filePath}");
                return null;
            }
            
            var text = await extractor!.ExtractTextAsync(filePath);
            
            return new FileTextResult
            {
                FilePath = Path.GetFullPath(filePath),
                Text = text ?? string.Empty,
                Success = true
            };
        }
        catch (Exception ex)
        {
            OnExtractionError(filePath, ex, $"Error extracting text from {filePath}: {ex.Message}");
            return null;
        }
    }
    
    private async IAsyncEnumerable<string> GetSupportedFilesFromDirectoryAsync(
        string directory, 
        int maxDepth, 
        bool recursive, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default,
        int currentDepth = 0)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        // Get files in current directory
        IEnumerable<string> directoryFiles = Directory.EnumerateFiles(directory);
        
        foreach (var file in directoryFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return file;
        }
        
        // Recursively process subdirectories if enabled and within depth limit
        if (recursive && currentDepth < maxDepth)
        {
            IEnumerable<string> subdirectories;
            try
            {
                subdirectories = Directory.EnumerateDirectories(directory);
            }
            catch (Exception ex)
            {
                OnExtractionError(directory, ex, $"Error accessing subdirectories in {directory}: {ex.Message}");
                yield break;
            }
            
            foreach (var subdirectory in subdirectories)
            {
                await foreach (var file in GetSupportedFilesFromDirectoryAsync(subdirectory, maxDepth, recursive, cancellationToken, currentDepth + 1))
                {
                    yield return file;
                }
            }
        }
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
