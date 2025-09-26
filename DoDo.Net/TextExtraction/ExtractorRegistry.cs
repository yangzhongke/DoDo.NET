namespace DoDo.Net.TextExtraction;

/// <summary>
/// Registry for managing text extractors without using reflection
/// </summary>
public class ExtractorRegistry
{
    private readonly Dictionary<string, ITextExtractor> _extractors = new(StringComparer.OrdinalIgnoreCase);
    
    /// <summary>
    /// Gets all supported file extensions
    /// </summary>
    public IReadOnlySet<string> SupportedExtensions => _extractors.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
    
    /// <summary>
    /// Registers a text extractor for specific file extensions
    /// </summary>
    public void RegisterExtractor(ITextExtractor extractor)
    {
        foreach (var extension in extractor.SupportedExtensions)
        {
            _extractors[extension] = extractor;
        }
    }
    
    /// <summary>
    /// Gets the appropriate extractor for a file extension
    /// </summary>
    public bool TryGetExtractor(string extension, out ITextExtractor? extractor)
    {
        return _extractors.TryGetValue(extension.ToLowerInvariant(), out extractor);
    }
    
    /// <summary>
    /// Checks if a file extension is supported
    /// </summary>
    public bool IsSupported(string extension)
    {
        return _extractors.ContainsKey(extension.ToLowerInvariant());
    }
}
