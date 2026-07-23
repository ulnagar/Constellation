namespace Constellation.Application.Interfaces.Services.Excel;

public sealed record ExcelColumn<T>(
    string Header,
    Func<T, object?> ValueSelector,
    ExcelColumnFormat Format = ExcelColumnFormat.Default);

public enum ExcelColumnFormat
{
    Default,
    Text,
    Date
}