namespace DoDo.Net.TextExtraction;

/// <summary>
/// Interface for text extractors that can extract text from specific file formats
/// </summary>
public interface ITextExtractor
{
    /// <summary>
    /// Determines if this extractor can handle the specified file
    /// </summary>
    /// <param name="filePath">The path to the file to check</param>
    /// <returns>True if this extractor can handle the file</returns>
    bool IsSupported(string filePath);
    
    /// <summary>
    /// Extracts text from a file asynchronously
    /// </summary>
    /// <param name="filePath">The path to the file to extract text from</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The extracted text</returns>
    Task<string> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default);
}
