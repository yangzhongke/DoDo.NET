using System.Text;
using UglyToad.PdfPig;

namespace DoDo.Net.Extractors;

/// <summary>
///     Extractor for PDF files using PdfPig (commercial-free alternative to iText7)
/// </summary>
public class PdfExtractor : ITextExtractor
{
    private static readonly HashSet<string> PdfExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf"
    };

    public bool IsSupported(string filePath)
    {
        return FileHelper.HasExtension(filePath, PdfExtensions);
    }

    public async Task<string> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var document = PdfDocument.Open(filePath);
                var textBuilder = new StringBuilder();

                foreach (var page in document.GetPages())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var pageText = page.Text;
                    if (!string.IsNullOrWhiteSpace(pageText))
                    {
                        textBuilder.AppendLine(pageText);
                        textBuilder.AppendLine(); // Add separation between pages
                    }
                }

                return textBuilder.ToString().Trim();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to extract text from PDF: {ex.Message}", ex);
            }
        }, cancellationToken);
    }
}