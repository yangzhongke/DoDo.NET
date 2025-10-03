namespace DoDo.Net;

public interface ITextExtractionService
{
    /// <summary>
    ///     Gets all registered extractors
    /// </summary>
    IEnumerable<ITextExtractor> RegisteredExtractors { get; }

    /// <summary>
    ///     Registers a custom text extractor
    /// </summary>
    void RegisterExtractor(ITextExtractor extractor);

    IAsyncEnumerable<FileTextResult> ReadFromFilesAsync(
        IEnumerable<string> files, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Extracts text from multiple files
    /// </summary>
    IAsyncEnumerable<FileTextResult> ReadFromFilesAsync(
        IAsyncEnumerable<string> files, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Extracts text from all supported files in a directory
    /// </summary>
    IAsyncEnumerable<FileTextResult> ReadFromDirectoryAsync(
        string directory,
        int maxDepth = int.MaxValue,
        bool recursive = true, CancellationToken cancellationToken = default);

    Task<string> ReadFromFileAsync(string filePath, CancellationToken cancellationToken = default);
}