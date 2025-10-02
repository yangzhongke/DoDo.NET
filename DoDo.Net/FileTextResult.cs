namespace DoDo.Net;

/// <summary>
///     Represents the result of text extraction from a file
/// </summary>
public record FileTextResult
{
    /// <summary>
    ///     The full path to the original file
    /// </summary>
    public required string FilePath { get; init; }

    /// <summary>
    ///     The extracted text content from the file
    /// </summary>
    public string? Text { get; init; }
}