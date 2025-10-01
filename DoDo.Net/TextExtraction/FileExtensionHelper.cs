namespace DoDo.Net.TextExtraction;

/// <summary>
/// Helper class for file extension operations and common extension sets
/// </summary>
public static class FileExtensionHelper
{
    /// <summary>
    /// Common text file extensions
    /// </summary>
    public static readonly HashSet<string> TextExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt", ".log", ".csv", ".tsv", ".json", ".xml", ".md", ".markdown", 
        ".yml", ".yaml", ".ini", ".cfg", ".config", ".env", ".gitignore",
        ".cs", ".js", ".ts", ".py", ".java", ".cpp", ".c", ".h", ".sql",
        ".ps1", ".bat", ".sh", ".properties"
    };

    /// <summary>
    /// HTML file extensions
    /// </summary>
    public static readonly HashSet<string> HtmlExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".html", ".htm"
    };

    /// <summary>
    /// PDF file extensions
    /// </summary>
    public static readonly HashSet<string> PdfExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf"
    };

    /// <summary>
    /// Word document extensions
    /// </summary>
    public static readonly HashSet<string> WordExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".docx", ".doc"
    };

    /// <summary>
    /// Excel document extensions
    /// </summary>
    public static readonly HashSet<string> ExcelExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".xlsx", ".xls", ".csv"
    };

    /// <summary>
    /// PowerPoint document extensions
    /// </summary>
    public static readonly HashSet<string> PowerPointExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pptx", ".ppt"
    };

    /// <summary>
    /// Checks if the file has one of the specified extensions
    /// </summary>
    /// <param name="filePath">The file path to check</param>
    /// <param name="extensions">The set of extensions to check against</param>
    /// <returns>True if the file has a matching extension</returns>
    public static bool HasExtension(string filePath, HashSet<string> extensions)
    {
        if (string.IsNullOrEmpty(filePath))
            return false;

        var extension = Path.GetExtension(filePath);
        return extensions.Contains(extension);
    }

    /// <summary>
    /// Gets the file extension from a file path
    /// </summary>
    /// <param name="filePath">The file path</param>
    /// <returns>The file extension (including the dot), or empty string if none</returns>
    public static string GetExtension(string filePath)
    {
        return string.IsNullOrEmpty(filePath) ? string.Empty : Path.GetExtension(filePath);
    }

    /// <summary>
    /// Checks if a file is likely a text-based file based on its extension
    /// </summary>
    /// <param name="filePath">The file path to check</param>
    /// <returns>True if the file appears to be text-based</returns>
    public static bool IsTextBasedFile(string filePath)
    {
        return HasExtension(filePath, TextExtensions) ||
               HasExtension(filePath, HtmlExtensions);
    }

    /// <summary>
    /// Gets the file size in bytes
    /// </summary>
    /// <param name="filePath">The file path</param>
    /// <returns>File size in bytes, or -1 if file doesn't exist</returns>
    public static long GetFileSize(string filePath)
    {
        try
        {
            return new FileInfo(filePath).Length;
        }
        catch
        {
            return -1;
        }
    }

    /// <summary>
    /// Checks if file size is under the specified limit
    /// </summary>
    /// <param name="filePath">The file path</param>
    /// <param name="maxSizeBytes">Maximum size in bytes</param>
    /// <returns>True if file is under the size limit</returns>
    public static bool IsFileSizeUnder(string filePath, long maxSizeBytes)
    {
        var fileSize = GetFileSize(filePath);
        return fileSize != -1 && fileSize < maxSizeBytes;
    }
}
