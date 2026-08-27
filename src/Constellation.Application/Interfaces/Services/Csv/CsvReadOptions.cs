namespace Constellation.Application.Interfaces.Services.Csv;

using System.Text;

public sealed record CsvReadOptions
{
    public bool HasHeaderRow { get; init; } = true;
    public char Delimiter { get; init; } = ',';
    public Encoding Encoding { get; init; } = Encoding.UTF8;
    public string[]? ExpectedHeaders { get; init; }
}