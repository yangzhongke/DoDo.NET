using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using System.Text;

namespace DoDo.Net.TextExtraction.Extractors;

/// <summary>
/// Extractor for Word documents using DocumentFormat.OpenXml
/// </summary>
public class WordExtractor : ITextExtractor
{
    private static readonly HashSet<string> WordExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".docx", ".doc"
    };
    
    public bool IsSupported(string filePath)
    {
        return FileExtensionHelper.HasExtension(filePath, WordExtensions);
    }

    public async Task<string> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var document = WordprocessingDocument.Open(filePath, false);
                var body = document.MainDocumentPart?.Document?.Body;
                
                if (body == null)
                    return string.Empty;
                
                var textBuilder = new StringBuilder();
                ExtractTextFromElement(body, textBuilder);
                
                return textBuilder.ToString().Trim();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to extract text from Word document: {ex.Message}", ex);
            }
        }, cancellationToken);
    }

    private static void ExtractTextFromElement(OpenXmlElement element, StringBuilder textBuilder)
    {
        foreach (var child in element.Elements())
        {
            if (child is Text textElement)
            {
                textBuilder.Append(textElement.Text);
            }
            else if (child is Paragraph)
            {
                ExtractTextFromElement(child, textBuilder);
                textBuilder.AppendLine();
            }
            else if (child is Run || child is Break)
            {
                ExtractTextFromElement(child, textBuilder);
            }
            else
            {
                ExtractTextFromElement(child, textBuilder);
            }
        }
    }
}
