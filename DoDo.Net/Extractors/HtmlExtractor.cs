using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Ude;

namespace DoDo.Net.Extractors;

/// <summary>
///     Extracts text from HTML files by removing HTML tags and decoding entities
/// </summary>
public class HtmlExtractor : ITextExtractor
{
    private static readonly HashSet<string> HtmlExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".html", ".htm"
    };

    public bool IsSupported(string filePath)
    {
        return FileHelper.HasExtension(filePath, HtmlExtensions);
    }

    public async Task<string> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default)
    {
        // Read file with encoding detection
        var fileBytes = await FileHelper.ReadAllBytesAsync(filePath, cancellationToken);

        var detector = new CharsetDetector();
        detector.Feed(fileBytes, 0, fileBytes.Length);
        detector.DataEnd();

        var encoding = Encoding.UTF8;
        if (detector.Charset != null)
        {
            try
            {
                encoding = Encoding.GetEncoding(detector.Charset);
            }
            catch
            {
                encoding = Encoding.UTF8;
            }
        }

        var htmlContent = encoding.GetString(fileBytes);

        // Load HTML document
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        // Remove script and style elements
        doc.DocumentNode.Descendants()
            .Where(n => n.Name == "script" || n.Name == "style")
            .ToList()
            .ForEach(n => n.Remove());

        // Extract text and decode HTML entities
        var text = doc.DocumentNode.InnerText;
        text = HtmlEntity.DeEntitize(text);

        // Clean up whitespace
        return CleanWhitespace(text);
    }

    private static string CleanWhitespace(string text)
    {
        // Replace multiple whitespace characters with single spaces
        return string.Join("\n", text.Split('\n')
            .Select(line => Regex.Replace(line.Trim(), @"\s+", " "))
            .Where(line => !string.IsNullOrEmpty(line)));
    }
}