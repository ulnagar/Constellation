namespace Constellation.Infrastructure.Services.Excel;

using Constellation.Application.Interfaces.Services.Excel;
using Constellation.Infrastructure.Models;
using OfficeOpenXml;

public sealed class EpplusExcelReader : IExcelReader
{
    public IExcelWorkbook OpenWorkbook(Stream fileStream)
    {
        var package = new ExcelPackage(fileStream);
        return new EpplusWorkbook(package);
    }

    public IExcelWorkbook OpenWorkbook(byte[] fileBytes)
    {
        using var stream = new MemoryStream(fileBytes);
        var package = new ExcelPackage(stream);
        return new EpplusWorkbook(package);
    }

    public IExcelWorksheet GetWorksheet(IExcelWorkbook workbook, string sheetName)
    {
        var epplusWorkbook = Unwrap(workbook);
        var sheet = epplusWorkbook.Package.Workbook.Worksheets[sheetName];

        if (sheet is null)
            throw new InvalidOperationException($"Worksheet '{sheetName}' was not found in the workbook.");

        return new EpplusWorksheet(sheet);
    }

    public IExcelWorksheet GetWorksheet(IExcelWorkbook workbook, int index = 0)
    {
        var epplusWorkbook = Unwrap(workbook);

        if (index < 0 || index >= epplusWorkbook.Package.Workbook.Worksheets.Count)
            throw new InvalidOperationException($"Worksheet index {index} is out of range.");

        var sheet = epplusWorkbook.Package.Workbook.Worksheets[index];
        return new EpplusWorksheet(sheet);
    }

    public IReadOnlyList<string> ReadHeaderRow(IExcelWorksheet sheet, int row = 1)
    {
        var epplusSheet = Unwrap(sheet).Worksheet;

        if (epplusSheet.Dimension is null)
            return Array.Empty<string>();

        int lastColumn = epplusSheet.Dimension.End.Column;
        var headers = new List<string>();

        for (int col = 1; col <= lastColumn; col++)
            headers.Add(epplusSheet.Cells[row, col].Text?.Trim() ?? string.Empty);

        return headers;
    }

    public string? ReadCellAsString(IExcelWorksheet sheet, int row, int column)
    {
        var epplusSheet = Unwrap(sheet).Worksheet;
        var value = epplusSheet.Cells[row, column].Text;

        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public int? ReadCellAsInt(IExcelWorksheet sheet, int row, int column)
    {
        var epplusSheet = Unwrap(sheet).Worksheet;
        var cell = epplusSheet.Cells[row, column];

        if (cell.Value is null)
            return null;

        if (cell.Value is double doubleValue)
            return (int)doubleValue;

        if (int.TryParse(cell.Text, out int parsed))
            return parsed;

        return null;
    }

    public decimal? ReadCellAsDecimal(IExcelWorksheet sheet, int row, int column)
    {
        var epplusSheet = Unwrap(sheet).Worksheet;
        var cell = epplusSheet.Cells[row, column];

        if (cell.Value is null)
            return null;

        if (cell.Value is double doubleValue)
            return (decimal)doubleValue;

        if (decimal.TryParse(cell.Text, out decimal parsed))
            return parsed;

        return null;
    }

    public DateTime? ReadCellAsDateTime(IExcelWorksheet sheet, int row, int column)
    {
        var epplusSheet = Unwrap(sheet).Worksheet;
        var cell = epplusSheet.Cells[row, column];

        if (cell.Value is null)
            return null;

        if (cell.Value is DateTime dateTimeValue)
            return dateTimeValue;

        // EPPlus stores dates as OLE Automation date serials (double) under the hood
        if (cell.Value is double doubleValue)
            return DateTime.FromOADate(doubleValue);

        if (DateTime.TryParse(cell.Text, out DateTime parsed))
            return parsed;

        return null;
    }

    public int GetLastUsedRow(IExcelWorksheet sheet)
    {
        var epplusSheet = Unwrap(sheet).Worksheet;
        return epplusSheet.Dimension?.End.Row ?? 0;
    }

    public int GetLastUsedColumn(IExcelWorksheet sheet)
    {
        var epplusSheet = Unwrap(sheet).Worksheet;
        return epplusSheet.Dimension?.End.Column ?? 0;
    }

    public IEnumerable<IReadOnlyDictionary<string, string?>> ReadRowsAsDictionaries(
        IExcelWorksheet sheet, int headerRow = 1, int firstDataRow = 2)
    {
        var epplusSheet = Unwrap(sheet).Worksheet;

        if (epplusSheet.Dimension is null)
            yield break;

        var headers = ReadHeaderRow(sheet, headerRow);
        int lastRow = epplusSheet.Dimension.End.Row;

        for (int row = firstDataRow; row <= lastRow; row++)
        {
            // Skip fully blank rows rather than yielding an all-null dictionary
            if (IsRowBlank(epplusSheet, row, headers.Count))
                continue;

            var rowValues = new Dictionary<string, string?>();

            for (int col = 0; col < headers.Count; col++)
            {
                string header = headers[col];
                if (string.IsNullOrEmpty(header))
                    continue;

                string? value = epplusSheet.Cells[row, col + 1].Text;
                rowValues[header] = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            }

            yield return rowValues;
        }
    }

    private static bool IsRowBlank(ExcelWorksheet sheet, int row, int columnCount)
    {
        for (int col = 1; col <= columnCount; col++)
        {
            if (!string.IsNullOrWhiteSpace(sheet.Cells[row, col].Text))
                return false;
        }

        return true;
    }

    private static EpplusWorkbook Unwrap(IExcelWorkbook workbook)
    {
        if (workbook is not EpplusWorkbook epplusWorkbook)
            throw new InvalidOperationException(
                $"Expected {nameof(EpplusWorkbook)} but received {workbook.GetType().Name}. " +
                "IExcelWorkbook instances must originate from IExcelReader.OpenWorkbook().");

        return epplusWorkbook;
    }

    private static EpplusWorksheet Unwrap(IExcelWorksheet sheet)
    {
        if (sheet is not EpplusWorksheet epplusWorksheet)
            throw new InvalidOperationException(
                $"Expected {nameof(EpplusWorksheet)} but received {sheet.GetType().Name}.");

        return epplusWorksheet;
    }
}