using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using A = DocumentFormat.OpenXml.Drawing;

namespace DoDo.Net.TextExtraction.Extractors;

/// <summary>
/// Extracts text from PowerPoint presentations (.pptx format) using OpenXml
/// </summary>
public class PowerPointExtractor : ITextExtractor
{
    public IReadOnlySet<string> SupportedExtensions { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".pptx"
    };

    public async Task<string> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var document = PresentationDocument.Open(filePath, false);
            var presentationPart = document.PresentationPart;
            
            if (presentationPart?.Presentation == null)
                return string.Empty;
                
            var text = new StringBuilder();
            var slideIdList = presentationPart.Presentation.SlideIdList;
            
            if (slideIdList != null)
            {
                int slideNumber = 1;
                foreach (var slideId in slideIdList.Elements<SlideId>())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    var slidePart = (SlidePart)presentationPart.GetPartById(slideId.RelationshipId!);
                    text.AppendLine($"=== Slide {slideNumber} ===");
                    text.AppendLine(ExtractTextFromSlide(slidePart));
                    text.AppendLine();
                    slideNumber++;
                }
            }
            
            return text.ToString();
        }, cancellationToken);
    }
    
    private static string ExtractTextFromSlide(SlidePart slidePart)
    {
        var text = new StringBuilder();
        
        if (slidePart.Slide?.CommonSlideData?.ShapeTree != null)
        {
            foreach (var shape in slidePart.Slide.CommonSlideData.ShapeTree.Elements())
            {
                if (shape is Shape shapeElement && shapeElement.TextBody != null)
                {
                    foreach (var paragraph in shapeElement.TextBody.Elements<A.Paragraph>())
                    {
                        var paragraphText = ExtractTextFromParagraph(paragraph);
                        if (!string.IsNullOrWhiteSpace(paragraphText))
                        {
                            text.AppendLine(paragraphText);
                        }
                    }
                }
            }
        }
        
        return text.ToString();
    }
    
    private static string ExtractTextFromParagraph(A.Paragraph paragraph)
    {
        var text = new StringBuilder();
        
        foreach (var run in paragraph.Elements<A.Run>())
        {
            if (run.Text != null)
            {
                text.Append(run.Text.Text);
            }
        }
        
        return text.ToString();
    }
}
