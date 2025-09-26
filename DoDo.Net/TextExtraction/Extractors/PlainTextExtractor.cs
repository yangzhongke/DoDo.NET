using System.Text;
using Ude;

namespace DoDo.Net.TextExtraction.Extractors;

/// <summary>
/// Extracts text from plain text files with automatic encoding detection
/// </summary>
public class PlainTextExtractor : ITextExtractor
{
    public IReadOnlySet<string> SupportedExtensions { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".txt", ".log", ".csv", ".tsv", ".json", ".xml", ".md", ".markdown", ".yml", ".yaml", ".ini", ".cfg", ".config"
    };

    public async Task<string> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default)
    {
        // Read file as bytes first for encoding detection
        byte[] fileBytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
        
        // Detect encoding
        var detector = new CharsetDetector();
        detector.Feed(fileBytes, 0, fileBytes.Length);
        detector.DataEnd();
        
        Encoding encoding = Encoding.UTF8; // Default fallback
        
        if (detector.Charset != null)
        {
            try
            {
                encoding = Encoding.GetEncoding(detector.Charset);
            }
            catch
            {
                // If the detected encoding is not supported, fall back to UTF-8
                encoding = Encoding.UTF8;
            }
        }
        
        // If detection fails or returns low confidence, try common encodings
        if (detector.Confidence < 0.7)
        {
            // Try to detect BOM
            if (fileBytes.Length >= 3 && fileBytes[0] == 0xEF && fileBytes[1] == 0xBB && fileBytes[2] == 0xBF)
            {
                encoding = Encoding.UTF8;
            }
            else if (fileBytes.Length >= 2 && fileBytes[0] == 0xFF && fileBytes[1] == 0xFE)
            {
                encoding = Encoding.Unicode; // UTF-16 LE
            }
            else if (fileBytes.Length >= 2 && fileBytes[0] == 0xFE && fileBytes[1] == 0xFF)
            {
                encoding = Encoding.BigEndianUnicode; // UTF-16 BE
            }
        }
        
        return encoding.GetString(fileBytes);
    }
}
