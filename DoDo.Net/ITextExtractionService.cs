namespace DoDo.Net;

public interface ITextExtractionService
{
    /// <summary>
    /// Gets all supported file extensions
    /// </summary>
    ISet<string> SupportedExtensions { get; }

    /// <summary>
    /// Registers a custom text extractor
    /// </summary>
    void RegisterExtractor(ITextExtractor extractor);

    /// <summary>
    /// Extracts text from multiple files
    /// </summary>
    IAsyncEnumerable<FileTextResult> ReadFromFilesAsync(string[] files, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts text from all supported files in a directory
    /// </summary>
    IAsyncEnumerable<FileTextResult> ReadFromDirectoryAsync(
        string directory, 
        int maxDepth = int.MaxValue, 
        bool recursive = true, CancellationToken cancellationToken = default);

    Task<FileTextResult> ReadFromFileAsync(string filePath, CancellationToken cancellationToken = default);
}