using System.Text;
using OfficeOpenXml;

namespace DoDo.Net.TextExtraction.Extractors;

/// <summary>
/// Extracts text from Excel files (.xlsx format) using EPPlus
/// </summary>
public class ExcelExtractor : ITextExtractor
{
    public IReadOnlySet<string> SupportedExtensions { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".xlsx", ".xls"
    };

    static ExcelExtractor()
    {
        // Set EPPlus license context for non-commercial use
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<string> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var package = new ExcelPackage(new FileInfo(filePath));
            var text = new StringBuilder();
            
            foreach (var worksheet in package.Workbook.Worksheets)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                text.AppendLine($"=== Worksheet: {worksheet.Name} ===");
                
                if (worksheet.Dimension != null)
                {
                    for (int row = worksheet.Dimension.Start.Row; row <= worksheet.Dimension.End.Row; row++)
                    {
                        var rowValues = new List<string>();
                        
                        for (int col = worksheet.Dimension.Start.Column; col <= worksheet.Dimension.End.Column; col++)
                        {
                            var cell = worksheet.Cells[row, col];
                            string cellValue = cell.Text ?? string.Empty;
                            rowValues.Add(cellValue);
                        }
                        
                        // Only add non-empty rows
                        if (rowValues.Any(v => !string.IsNullOrWhiteSpace(v)))
                        {
                            text.AppendLine(string.Join("\t", rowValues));
                        }
                    }
                }
                
                text.AppendLine();
            }
            
            return text.ToString();
        }, cancellationToken);
    }
}
