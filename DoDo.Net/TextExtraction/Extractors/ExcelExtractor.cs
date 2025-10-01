using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using System.Text;
using System.Globalization;

namespace DoDo.Net.TextExtraction.Extractors;

/// <summary>
/// Extractor for Excel files using NPOI (commercial-free alternative to EPPlus)
/// Supports both .xls (HSSF) and .xlsx (XSSF) formats
/// </summary>
public class ExcelExtractor : ITextExtractor
{
    public IReadOnlySet<string> SupportedExtensions { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".xls", ".xlsx", ".csv"
    };

    public async Task<string> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                var extension = Path.GetExtension(filePath).ToLowerInvariant();
                
                if (extension == ".csv")
                {
                    return ExtractFromCsv(filePath);
                }
                
                return ExtractFromExcel(filePath, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to extract text from Excel file: {ex.Message}", ex);
            }
        }, cancellationToken);
    }

    private string ExtractFromExcel(string filePath, CancellationToken cancellationToken = default)
    {
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        
        IWorkbook workbook;
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        
        // Create appropriate workbook based on file extension
        if (extension == ".xlsx")
        {
            workbook = new XSSFWorkbook(fileStream);
        }
        else if (extension == ".xls")
        {
            workbook = new HSSFWorkbook(fileStream);
        }
        else
        {
            throw new NotSupportedException($"Unsupported Excel format: {extension}");
        }

        using (workbook)
        {
            var textBuilder = new StringBuilder();
            
            for (int sheetIndex = 0; sheetIndex < workbook.NumberOfSheets; sheetIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                var sheet = workbook.GetSheetAt(sheetIndex);
                
                if (workbook.NumberOfSheets > 1)
                {
                    textBuilder.AppendLine($"=== Sheet: {sheet.SheetName} ===");
                }
                
                foreach (IRow row in sheet)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    var rowText = new List<string>();
                    
                    foreach (ICell cell in row)
                    {
                        var cellValue = GetCellValueAsString(cell);
                        if (!string.IsNullOrWhiteSpace(cellValue))
                        {
                            rowText.Add(cellValue);
                        }
                    }
                    
                    if (rowText.Count > 0)
                    {
                        textBuilder.AppendLine(string.Join("\t", rowText));
                    }
                }
                
                if (sheetIndex < workbook.NumberOfSheets - 1)
                {
                    textBuilder.AppendLine();
                }
            }
            
            return textBuilder.ToString().Trim();
        }
    }

    private string ExtractFromCsv(string filePath)
    {
        return File.ReadAllText(filePath);
    }

    private string GetCellValueAsString(ICell cell)
    {
        string? value = cell.CellType switch
        {
            CellType.String => cell.StringCellValue,
            CellType.Numeric => DateUtil.IsCellDateFormatted(cell) 
                ? cell.DateCellValue?.ToString("yyyy-MM-dd HH:mm:ss")
                : cell.NumericCellValue.ToString(CultureInfo.InvariantCulture),
            CellType.Boolean => cell.BooleanCellValue.ToString(CultureInfo.InvariantCulture),
            CellType.Formula => GetFormulaCellValue(cell),
            CellType.Blank => string.Empty,
            _ => cell.ToString() ?? string.Empty
        };
        return value ?? string.Empty;
    }

    private string? GetFormulaCellValue(ICell cell)
    {
        try
        {
            string? value = cell.CachedFormulaResultType switch
            {
                CellType.String => cell.StringCellValue,
                CellType.Numeric => DateUtil.IsCellDateFormatted(cell)
                    ? cell.DateCellValue?.ToString("yyyy-MM-dd HH:mm:ss")
                    : cell.NumericCellValue.ToString(CultureInfo.InvariantCulture),
                CellType.Boolean => cell.BooleanCellValue.ToString(CultureInfo.InvariantCulture),
                _ => cell.ToString() ?? string.Empty
            };
            return value ?? string.Empty;
        }
        catch
        {
            // If formula evaluation fails, return the formula itself
            return cell.CellFormula;
        }
    }
}
