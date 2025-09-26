namespace DoDo.Net.TextExtraction;

/// <summary>
/// Interface for text extractors that can extract text from specific file formats
/// </summary>
public interface ITextExtractor
{
    /// <summary>
    /// The file extensions this extractor can handle (e.g., ".pdf", ".txt")
    /// </summary>
    IReadOnlySet<string> SupportedExtensions { get; }
    
    /// <summary>
    /// Extracts text from a file asynchronously
    /// </summary>
    /// <param name="filePath">The path to the file to extract text from</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The extracted text</returns>
    Task<string> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default);
}
