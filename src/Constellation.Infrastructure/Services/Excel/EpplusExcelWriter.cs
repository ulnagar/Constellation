namespace Constellation.Infrastructure.Services.Excel;

using Application.Interfaces.Services.Excel;
using Models;
using OfficeOpenXml;
using OfficeOpenXml.DataValidation;
using OfficeOpenXml.DataValidation.Contracts;
using OfficeOpenXml.Style;
using System.Drawing;

public sealed class EpplusExcelWriter : IExcelWriter
{
    public IExcelWorkbook CreateWorkbook()
    {
        ExcelPackage package = new ExcelPackage();
        return new EpplusWorkbook(package);
    }

    public IExcelWorksheet AddWorksheet(IExcelWorkbook workbook, string sheetName)
    {
        EpplusWorkbook epplusWorkbook = Unwrap(workbook);
        ExcelWorksheet? sheet = epplusWorkbook.Package.Workbook.Worksheets.Add(sheetName);
        return new EpplusWorksheet(sheet);
    }

    public void WriteHeaderRow(IExcelWorksheet sheet, IEnumerable<string> headers, int row = 1)
    {
        ExcelWorksheet epplusSheet = Unwrap(sheet).Worksheet;

        int column = 1;
        foreach (string header in headers)
        {
            epplusSheet.Cells[row, column].Value = header;
            column++;
        }
    }

    public void WriteRow(IExcelWorksheet sheet, int row, IEnumerable<object?> values)
    {
        ExcelWorksheet epplusSheet = Unwrap(sheet).Worksheet;

        int column = 1;
        foreach (object? value in values)
        {
            epplusSheet.Cells[row, column].Value = value;
            column++;
        }
    }

    public void WriteCell(IExcelWorksheet sheet, int row, int column, object? value)
    {
        ExcelWorksheet epplusSheet = Unwrap(sheet).Worksheet;
        epplusSheet.Cells[row, column].Value = value;
    }

    public void WriteRange<T>(
        IExcelWorksheet sheet,
        int startRow,
        IEnumerable<T> items,
        params (string Header, Func<T, object?> ValueSelector)[] columns)
    {
        ExcelWorksheet epplusSheet = Unwrap(sheet).Worksheet;

        // Header row goes immediately above the first data row
        for (int col = 0; col < columns.Length; col++)
        {
            epplusSheet.Cells[startRow - 1, col + 1].Value = columns[col].Header;
        }

        int row = startRow;
        foreach (T item in items)
        {
            for (int col = 0; col < columns.Length; col++)
            {
                epplusSheet.Cells[row, col + 1].Value = columns[col].ValueSelector(item);
            }
            row++;
        }
    }

    public void ApplyHeaderStyle(IExcelWorksheet sheet, int row)
    {
        ExcelWorksheet epplusSheet = Unwrap(sheet).Worksheet;

        if (epplusSheet.Dimension is null)
            return;

        int lastColumn = epplusSheet.Dimension.End.Column;
        ExcelRange? headerRange = epplusSheet.Cells[row, 1, row, lastColumn];

        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
        headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
        headerRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
    }

    public void AutoFitColumns(IExcelWorksheet sheet)
    {
        ExcelWorksheet epplusSheet = Unwrap(sheet).Worksheet;

        if (epplusSheet.Dimension is null)
            return;

        epplusSheet.Cells[epplusSheet.Dimension.Address].AutoFitColumns();
    }

    public void SetColumnWidth(IExcelWorksheet sheet, int column, double width)
    {
        ExcelWorksheet epplusSheet = Unwrap(sheet).Worksheet;
        epplusSheet.Column(column).Width = width;
    }

    public void ApplyNumberFormat(IExcelWorksheet sheet, int column, string format)
    {
        ExcelWorksheet epplusSheet = Unwrap(sheet).Worksheet;

        if (epplusSheet.Dimension is null)
            return;

        int lastRow = epplusSheet.Dimension.End.Row;
        epplusSheet.Cells[1, column, lastRow, column].Style.Numberformat.Format = format;
    }

    public void FreezePanes(IExcelWorksheet sheet, int row, int column)
    {
        ExcelWorksheet epplusSheet = Unwrap(sheet).Worksheet;
        epplusSheet.View.FreezePanes(row, column);
    }

    public void AddDataValidationDropdown(IExcelWorksheet sheet, string cellRange, IEnumerable<string> options)
    {
        ExcelWorksheet epplusSheet = Unwrap(sheet).Worksheet;

        IExcelDataValidationList? validation = epplusSheet.DataValidations.AddListValidation(cellRange);
        foreach (string option in options)
        {
            validation.Formula.Values.Add(option);
        }

        validation.ShowErrorMessage = true;
        validation.ErrorStyle = ExcelDataValidationWarningStyle.stop;
        validation.Error = "Please select a value from the list";
    }

    public void AddAutoFilter(IExcelWorksheet sheet, string range)
    {
        ExcelWorksheet epplusSheet = Unwrap(sheet).Worksheet;
        epplusSheet.Cells[range].AutoFilter = true;
    }

    public byte[] GetAsByteArray(IExcelWorkbook workbook)
    {
        EpplusWorkbook epplusWorkbook = Unwrap(workbook);
        return epplusWorkbook.Package.GetAsByteArray();
    }

    public MemoryStream GetAsStream(IExcelWorkbook workbook)
    {
        EpplusWorkbook epplusWorkbook = Unwrap(workbook);
        MemoryStream stream = new MemoryStream();
        epplusWorkbook.Package.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    private static EpplusWorkbook Unwrap(IExcelWorkbook workbook)
    {
        if (workbook is not EpplusWorkbook epplusWorkbook)
            throw new InvalidOperationException(
                $"Expected {nameof(EpplusWorkbook)} but received {workbook.GetType().Name}. " +
                "IExcelWorkbook instances must originate from IExcelWriter.CreateWorkbook().");

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