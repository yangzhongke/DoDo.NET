namespace DoDo.Net;

/// <summary>
/// Event arguments for text extraction errors
/// </summary>
public class TextExtractionErrorEventArgs : EventArgs
{
    /// <summary>
    /// The file path that caused the error
    /// </summary>
    public required string FilePath { get; init; }
    
    /// <summary>
    /// The exception that occurred
    /// </summary>
    public required Exception Exception { get; init; }
    
    /// <summary>
    /// A descriptive error message
    /// </summary>
    public required string Message { get; init; }
}
