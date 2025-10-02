namespace DoDo.Net.Extractors;

/// <summary>
/// Extracts text from plain text files with automatic encoding detection.
/// Acts as a fallback for any file under 1MB that other extractors don't support.
/// </summary>
public class FallbackPlainTextExtractor : AbstractPlainTextExtractor
{
    private const long MaxFileSizeBytes = 1024 * 1024; // 1MB

    public override bool IsSupported(string filePath)
    {
        // For unknown extensions, act as fallback for files under 1MB
        return new FileInfo(filePath).Length < MaxFileSizeBytes;
    }
}
