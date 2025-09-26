using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DoDo.Net.TextExtraction.Extractors;

/// <summary>
/// Extracts text from Word documents (.docx format) using OpenXml
/// </summary>
public class WordExtractor : ITextExtractor
{
    public IReadOnlySet<string> SupportedExtensions { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".docx"
    };

    public async Task<string> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var document = WordprocessingDocument.Open(filePath, false);
            var body = document.MainDocumentPart?.Document?.Body;
            
            if (body == null)
                return string.Empty;
                
            return ExtractTextFromBody(body);
        }, cancellationToken);
    }
    
    private static string ExtractTextFromBody(Body body)
    {
        var text = new StringBuilder();
        
        foreach (var element in body.Elements())
        {
            if (element is Paragraph paragraph)
            {
                text.AppendLine(ExtractTextFromParagraph(paragraph));
            }
            else if (element is Table table)
            {
                text.AppendLine(ExtractTextFromTable(table));
            }
        }
        
        return text.ToString();
    }
    
    private static string ExtractTextFromParagraph(Paragraph paragraph)
    {
        var text = new StringBuilder();
        
        foreach (var run in paragraph.Elements<Run>())
        {
            foreach (var textElement in run.Elements<Text>())
            {
                text.Append(textElement.Text);
            }
        }
        
        return text.ToString();
    }
    
    private static string ExtractTextFromTable(Table table)
    {
        var text = new StringBuilder();
        
        foreach (var row in table.Elements<TableRow>())
        {
            var rowText = new List<string>();
            foreach (var cell in row.Elements<TableCell>())
            {
                var cellText = new StringBuilder();
                foreach (var paragraph in cell.Elements<Paragraph>())
                {
                    cellText.Append(ExtractTextFromParagraph(paragraph));
                }
                rowText.Add(cellText.ToString().Trim());
            }
            text.AppendLine(string.Join("\t", rowText));
        }
        
        return text.ToString();
    }
}
