namespace DoDo.Net;

/// <summary>
/// Helper class for file extension operations and common extension sets
/// </summary>
public static class FileExtensionHelper
{
    /// <summary>
    /// Checks if the file has one of the specified extensions
    /// </summary>
    /// <param name="filePath">The file path to check</param>
    /// <param name="extensions">The set of extensions to check against</param>
    /// <returns>True if the file has a matching extension</returns>
    public static bool HasExtension(string filePath, IReadOnlySet<string> extensions)
    {
        if (string.IsNullOrEmpty(filePath))
            return false;

        var extension = Path.GetExtension(filePath);
        return extensions.Contains(extension);
    }
}
