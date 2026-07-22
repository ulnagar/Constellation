namespace Constellation.Application.Interfaces.Services.Excel;

public interface IExcelReader
{
    // Loading
    IExcelWorkbook OpenWorkbook(Stream fileStream);
    IExcelWorkbook OpenWorkbook(byte[] fileBytes);
    IExcelWorksheet GetWorksheet(IExcelWorkbook workbook, string sheetName);
    IExcelWorksheet GetWorksheet(IExcelWorkbook workbook, int index = 0);

    // Reading
    IReadOnlyList<string> ReadHeaderRow(IExcelWorksheet sheet, int row = 1);
    string? ReadCellAsString(IExcelWorksheet sheet, int row, int column);
    int? ReadCellAsInt(IExcelWorksheet sheet, int row, int column);
    decimal? ReadCellAsDecimal(IExcelWorksheet sheet, int row, int column);
    DateTime? ReadCellAsDateTime(IExcelWorksheet sheet, int row, int column);
    int GetLastUsedRow(IExcelWorksheet sheet);
    int GetLastUsedColumn(IExcelWorksheet sheet);

    // Row-based iteration (the shape most import handlers actually want)
    IEnumerable<IReadOnlyDictionary<string, string?>> ReadRowsAsDictionaries(
        IExcelWorksheet sheet, int headerRow = 1, int firstDataRow = 2);
}