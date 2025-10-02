namespace DoDo.Net;

/// <summary>
///     Helper class for file extension operations and common extension sets
/// </summary>
public static class FileHelper
{
    /// <summary>
    ///     Checks if the file has one of the specified extensions
    /// </summary>
    /// <param name="filePath">The file path to check</param>
    /// <param name="extensions">The set of extensions to check against</param>
    /// <returns>True if the file has a matching extension</returns>
    public static bool HasExtension(string filePath, ISet<string> extensions)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return false;
        }

        var extension = Path.GetExtension(filePath);
        return extensions.Contains(extension);
    }

    /// <summary>
    ///     Asynchronously reads all bytes from a file
    /// </summary>
    /// <param name="filePath">The file path to read from</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A byte array containing the contents of the file</returns>
    /// <exception cref="ArgumentException">Thrown when filePath is null or empty</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist</exception>
    public static async Task<byte[]> ReadAllBytesAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}", filePath);
        }

        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        var buffer = new byte[fileStream.Length];
        _ = await fileStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
        return buffer;
    }
}