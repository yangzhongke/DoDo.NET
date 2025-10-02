using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;

namespace DoDo.Net.Extractors;

/// <summary>
///     Extractor for PowerPoint presentations using DocumentFormat.OpenXml
/// </summary>
public class PowerPointExtractor : ITextExtractor
{
    private static readonly HashSet<string> PowerPointExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pptx", ".ppt"
    };

    public bool IsSupported(string filePath)
    {
        return FileHelper.HasExtension(filePath, PowerPointExtensions);
    }

    public async Task<string> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var document = PresentationDocument.Open(filePath, false);
                var presentation = document.PresentationPart?.Presentation;

                if (presentation?.SlideIdList == null)
                {
                    return string.Empty;
                }

                var textBuilder = new StringBuilder();

                foreach (var slideId in presentation.SlideIdList.Elements<SlideId>())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var slidePart = (SlidePart)document.PresentationPart!.GetPartById(slideId.RelationshipId!);
                    ExtractTextFromSlide(slidePart, textBuilder);
                    textBuilder.AppendLine();
                }

                return textBuilder.ToString().Trim();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to extract text from PowerPoint: {ex.Message}", ex);
            }
        }, cancellationToken);
    }

    private static void ExtractTextFromSlide(SlidePart slidePart, StringBuilder textBuilder)
    {
        var slide = slidePart.Slide;

        foreach (var textElement in slide.Descendants<DocumentFormat.OpenXml.Drawing.Text>())
        {
            if (!string.IsNullOrWhiteSpace(textElement.Text))
            {
                textBuilder.AppendLine(textElement.Text);
            }
        }
    }
}