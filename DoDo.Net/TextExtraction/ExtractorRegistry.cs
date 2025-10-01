namespace DoDo.Net.TextExtraction;

/// <summary>
/// Registry for managing text extractors with ordered registration and forward search
/// </summary>
public class ExtractorRegistry
{
    private readonly List<ITextExtractor> _extractors = new();
    
    /// <summary>
    /// Registers a text extractor. Newly registered extractors are checked first.
    /// </summary>
    /// <param name="extractor">The extractor to register</param>
    public void RegisterExtractor(ITextExtractor extractor)
    {
        if (extractor == null)
            throw new ArgumentNullException(nameof(extractor));
            
        _extractors.Add(extractor);
    }
    
    /// <summary>
    /// Tries to get an extractor for the specified file.
    /// Searches forward through registered extractors (newest first).
    /// </summary>
    /// <param name="filePath">The file path to find an extractor for</param>
    /// <param name="extractor">The found extractor, or null if none found</param>
    /// <returns>True if an extractor was found</returns>
    public bool TryGetExtractor(string filePath, out ITextExtractor? extractor)
    {
        // Search forward. newest registrations first so that new extractors can override specific files that older extractors handle
        for (int i = _extractors.Count - 1; i >= 0; i--)
        {
            var currentExtractor = _extractors[i];
            if (currentExtractor.IsSupported(filePath))
            {
                extractor = currentExtractor;
                return true;
            }
        }
        
        extractor = null;
        return false;
    }
    
    /// <summary>
    /// Gets all registered extractors (in registration order)
    /// </summary>
    public IEnumerable<ITextExtractor> GetAllExtractors()
    {
        return _extractors.AsReadOnly();
    }
}
