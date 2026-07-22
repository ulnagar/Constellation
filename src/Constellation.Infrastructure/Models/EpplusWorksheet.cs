namespace Constellation.Infrastructure.Models;

using Application.Interfaces.Services.Excel;
using OfficeOpenXml;

internal sealed class EpplusWorksheet : IExcelWorksheet
{
    public ExcelWorksheet Worksheet { get; }

    public string Name => Worksheet.Name;

    public EpplusWorksheet(ExcelWorksheet worksheet)
    {
        Worksheet = worksheet;
    }
}