namespace Constellation.Application.Interfaces.Services.Excel;

using System;
using System.Collections.Generic;

public interface IExcelWriter
{
    // Workbook / worksheet lifecycle
    IExcelWorkbook CreateWorkbook();
    IExcelWorksheet AddWorksheet(IExcelWorkbook workbook, string sheetName);

    // Writing
    void WriteHeaderRow(IExcelWorksheet sheet, IEnumerable<string> headers, int row = 1);
    void WriteRow(IExcelWorksheet sheet, int row, IEnumerable<object?> values);
    void WriteCell(IExcelWorksheet sheet, int row, int column, object? value);
    void WriteRange<T>(IExcelWorksheet sheet, int startRow, IEnumerable<T> items,
        params (string Header, Func<T, object?> ValueSelector)[] columns);

    // Formatting (EPPlus-specific strengths worth exposing)
    void ApplyHeaderStyle(IExcelWorksheet sheet, int row);
    void AutoFitColumns(IExcelWorksheet sheet);
    void SetColumnWidth(IExcelWorksheet sheet, int column, double width);
    void ApplyNumberFormat(IExcelWorksheet sheet, int column, string format); // e.g. dd/MM/yyyy
    void FreezePanes(IExcelWorksheet sheet, int row, int column);
    void AddDataValidationDropdown(IExcelWorksheet sheet, string cellRange, IEnumerable<string> options);
    void AddAutoFilter(IExcelWorksheet sheet, string range);

    // Output
    byte[] GetAsByteArray(IExcelWorkbook workbook);
    MemoryStream GetAsStream(IExcelWorkbook workbook);
}