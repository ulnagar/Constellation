namespace Constellation.Infrastructure.Models;

using Application.Interfaces.Services.Excel;
using OfficeOpenXml;

// internal: nothing outside Infrastructure should reference this type directly
internal sealed class EpplusWorkbook : IExcelWorkbook
{
    public ExcelPackage Package { get; }

    public EpplusWorkbook(ExcelPackage package)
    {
        Package = package;
    }

    public void Dispose() => Package.Dispose();
}