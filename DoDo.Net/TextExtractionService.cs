using System.Runtime.CompilerServices;
using DoDo.Net.Extractors;
using DoDo.Net.Internal;
using Microsoft.Extensions.Logging;

namespace DoDo.Net;

/// <summary>
///     Main service for extracting text from various file formats
/// </summary>
public class TextExtractionService : ITextExtractionService
{
    private readonly ILogger _logger;
    private readonly ExtractionOptions _options;
    private readonly ExtractorRegistry _registry;

    public TextExtractionService(ILogger? logger = null, ExtractionOptions? options = null)
    {
        _registry = new ExtractorRegistry();
        _logger = logger ?? new ConsoleLogger();
        _options = options ?? ExtractionOptions.Default;

        RegisterDefaultExtractors();
    }

    /// <summary>
    ///     Gets all registered extractors
    /// </summary>
    public IEnumerable<ITextExtractor> RegisteredExtractors => _registry.GetAllExtractors();

    /// <summary>
    ///     Registers a custom text extractor
    /// </summary>
    public void RegisterExtractor(ITextExtractor extractor)
    {
        _registry.RegisterExtractor(extractor);
    }

    public IAsyncEnumerable<FileTextResult> ReadFromFilesAsync(
        IEnumerable<string> files, CancellationToken cancellationToken = default)
    {
        return ReadFromFilesAsync(files.ToAsyncEnumerable(), cancellationToken);
    }

    /// <summary>
    ///     Extracts text from multiple files
    /// </summary>
    public async IAsyncEnumerable<FileTextResult> ReadFromFilesAsync(
        IAsyncEnumerable<string> files, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var semaphore = new SemaphoreSlim(_options.ParallelCount);
        var activeTasks = new List<Task<FileTextResult?>>();
        var maxConcurrentTasks = _options.ParallelCount * 2; // Limit concurrent tasks to avoid overwhelming the system

        // Start all tasks
        await foreach (var filePath in files.WithCancellation(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var task = ReadFileAsync(filePath, semaphore, cancellationToken);
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

        // Yield results as they complete
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

        semaphore.Dispose();
    }

    /// <summary>
    ///     Extracts text from all supported files in a directory
    /// </summary>
    public IAsyncEnumerable<FileTextResult> ReadFromDirectoryAsync(
        string directory,
        int maxDepth = int.MaxValue,
        bool recursive = true, CancellationToken cancellationToken = default)
    {
        var files = GetSupportedFilesFromDirectoryAsync(directory, maxDepth, recursive,
            cancellationToken);
        return ReadFromFilesAsync(files, cancellationToken);
    }

    private void RegisterDefaultExtractors()
    {
        // Register PlainTextExtractor first so it acts as fallback for files under 1MB
        _registry.RegisterExtractor(new FallbackPlainTextExtractor(_options));

        // Register specific extractors first, PlainTextExtractor last as fallback
        // This order ensures newly registered extractors override older ones (forward search)
        _registry.RegisterExtractor(new HtmlExtractor());
        _registry.RegisterExtractor(new PdfExtractor());
        _registry.RegisterExtractor(new WordExtractor());
        _registry.RegisterExtractor(new ExcelExtractor());
        _registry.RegisterExtractor(new PowerPointExtractor());
        _registry.RegisterExtractor(new PlainTextExtractor());
    }

    public async Task<string> ReadFromFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var result = await ReadFileAsync(filePath, null, cancellationToken);
        return result?.Text ?? string.Empty;
    }

    private async Task<FileTextResult?> ReadFileAsync(string filePath, SemaphoreSlim? semaphore = null,
        CancellationToken cancellationToken = default)
    {
        if (semaphore is not null)
        {
            await semaphore.WaitAsync(cancellationToken);
        }

        FileTextResult? ret;

        try
        {
            if (!_registry.TryGetExtractor(filePath, out var extractor))
            {
                OnExtractionError(filePath,
                    new NotSupportedException($"No extractor available for file: {filePath}"),
                    $"No extractor available for file: {filePath}");
                ret = null;
            }
            else
            {
                var text = await extractor!.ExtractTextAsync(filePath, cancellationToken);

                ret = new FileTextResult
                {
                    FilePath = Path.GetFullPath(filePath),
                    Text = text
                };
            }
        }
        catch (Exception ex)
        {
            if (_options.ErrorHandling == ExtractionErrorHandling.ThrowOnFirstError)
            {
                throw;
            }

            OnExtractionError(filePath, ex, $"Error extracting text from {filePath}: {ex.Message}");
            ret = null;
        }
        finally
        {
            if (semaphore is not null)
            {
                semaphore.Release();
            }
        }

        return ret;
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
        var directoryFiles = Directory.EnumerateFiles(directory);

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
                if (_options.ErrorHandling == ExtractionErrorHandling.ThrowOnFirstError)
                {
                    throw;
                }

                OnExtractionError(directory, ex, $"Error accessing subdirectories in {directory}: {ex.Message}");
                yield break;
            }

            foreach (var subdirectory in subdirectories)
            {
                await foreach (var file in GetSupportedFilesFromDirectoryAsync(subdirectory, maxDepth, recursive,
                                   cancellationToken, currentDepth + 1))
                {
                    yield return file;
                }
            }
        }
    }

    private void OnExtractionError(string filePath, Exception exception, string message)
    {
        _logger.LogError(exception, "Error processing file {FilePath}: {Message}", filePath, message);
    }
}