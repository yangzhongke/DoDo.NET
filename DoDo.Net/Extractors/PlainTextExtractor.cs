namespace DoDo.Net.Extractors;

public class PlainTextExtractor : AbstractPlainTextExtractor
{
    /// <summary>
    /// Common text file extensions
    /// </summary>
    private static readonly HashSet<string> TextExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt", ".log", ".csv", ".tsv", ".json", ".xml", ".md", ".markdown", 
        ".yml", ".yaml", ".ini", ".cfg", ".config", ".env", ".gitignore",
        ".cs", ".js", ".ts", ".py", ".java", ".cpp", ".c", ".h", ".sql",
        ".ps1", ".bat", ".sh", ".properties"
    };
    
    public override bool IsSupported(string filePath)
    {
        return FileExtensionHelper.HasExtension(filePath, TextExtensions);
    }
}
