namespace DoDo.Net.TextExtraction;

/// <summary>
/// Represents the result of text extraction from a file
/// </summary>
public record FileTextResult
{
    /// <summary>
    /// The full path to the original file
    /// </summary>
    public required string FilePath { get; init; }
    
    /// <summary>
    /// The extracted text content from the file
    /// </summary>
    public required string Text { get; init; }
    
    /// <summary>
    /// The file extension (for reference)
    /// </summary>
    public string Extension => Path.GetExtension(FilePath).ToLowerInvariant();
    
    /// <summary>
    /// The file name without path
    /// </summary>
    public string FileName => Path.GetFileName(FilePath);
}
