using System.Text;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

namespace DoDo.Net.TextExtraction.Extractors;

/// <summary>
/// Extracts text from PDF files using iText7
/// </summary>
public class PdfExtractor : ITextExtractor
{
    public IReadOnlySet<string> SupportedExtensions { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf"
    };

    public async Task<string> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var text = new StringBuilder();
            
            using var pdfReader = new PdfReader(filePath);
            using var pdfDocument = new PdfDocument(pdfReader);
            
            int numberOfPages = pdfDocument.GetNumberOfPages();
            
            for (int i = 1; i <= numberOfPages; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                var page = pdfDocument.GetPage(i);
                string pageText = PdfTextExtractor.GetTextFromPage(page);
                
                if (!string.IsNullOrWhiteSpace(pageText))
                {
                    text.AppendLine(pageText);
                }
            }
            
            return text.ToString();
        }, cancellationToken);
    }
}
