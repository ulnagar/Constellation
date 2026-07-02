namespace Constellation.Infrastructure.Services;

using Application.Common.Errors;
using Application.Interfaces.Services;
using Application.Models.ImportCache;
using Core.Abstractions.Services;
using Core.Shared;
using CsvHelper;
using CsvHelper.Configuration;
using ExcelDataReader;
using OfficeOpenXml;
using System;
using System.Data;
using System.Globalization;
using System.Text;

internal sealed class ImportService : IImportService
{
    private readonly IImportStagingCache _stagingCache;
    private readonly ICurrentUserService _currentUserService;

    private enum ImportFileFormat { Csv, Xls, Xlsx, Unknown }

    private static readonly byte[] ZipSignature = [0x50, 0x4B, 0x03, 0x04];
    private static readonly byte[] OleSignature = [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1];

    public ImportService(
        IImportStagingCache stagingCache,
        ICurrentUserService currentUserService)
    {
        _stagingCache = stagingCache;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> StageImportFile(
        MemoryStream stream, 
        string fileName,
        CancellationToken cancellationToken = default)
    {
        ImportFileFormat format = DetectFileFormat(stream, fileName);

        Result<(List<string> Headers, List<StagedImportRow> Rows)> result = format switch
        {
            ImportFileFormat.Xlsx => await ParseXlsx(stream, cancellationToken),
            ImportFileFormat.Xls => await ParseXls(stream, cancellationToken),
            ImportFileFormat.Csv => await ParseCsv(stream, cancellationToken),
            _ => Result.Failure<(List<string> Headers, List<StagedImportRow> Rows)>(ImportErrors.InvalidFileSignature)
        };

        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);

        StagedImport staged = new()
        {
            Token = Guid.NewGuid(),
            OriginalFileName = fileName,
            Headers = result.Value.Headers,
            Rows = result.Value.Rows,
            UploadedAtUtc = DateTime.UtcNow,
            UploadedBy = _currentUserService.UserName
        };

        Guid token = _stagingCache.Stage(staged);
        return Result.Success(token);
    }

    private static async Task<Result<(List<string> Headers, List<StagedImportRow> Rows)>> ParseXlsx(
        MemoryStream stream,
        CancellationToken cancellationToken = default)
    {
        using ExcelPackage excel = new(stream);
        if (excel.Workbook.Worksheets.Count == 0)
            return Result.Failure<(List<string>, List<StagedImportRow>)>(ImportErrors.FileAppearsEmpty);

        ExcelWorksheet worksheet = excel.Workbook.Worksheets[0];

        if (worksheet.Dimension is null)
            return Result.Failure<(List<string>, List<StagedImportRow>)>(ImportErrors.FileAppearsEmpty);

        int numCols = worksheet.Dimension.Columns;
        int numRows = worksheet.Dimension.Rows;

        List<string> headers = [];
        List<StagedImportRow> rows = [];

        for (int col = 1; col <= numCols; col++)
        {
            string? rawHeader = worksheet.Cells[1, col].GetCellValue<string>();
            string trimmed = (rawHeader ?? string.Empty).Trim();

            if (!string.IsNullOrWhiteSpace(trimmed) && headers.Contains(trimmed))
                return Result.Failure<(List<string>, List<StagedImportRow>)>(ImportErrors.DuplicateHeaderValuesDetected);

            headers.Add(ResolveHeaderName(rawHeader, col, headers));
        }

        for (int row = 2; row <= numRows; row++)
        {
            Dictionary<string, string?> values = [];

            for (int col = 1; col <= numCols; col++)
            {
                string header = headers[col - 1]; // reuse validated headers instead of re-reading row 1
                string? value = worksheet.Cells[row, col].GetCellValue<string>();

                bool success = values.TryAdd(header, value);

                if (!success)
                    return Result.Failure<(List<string> Headers, List<StagedImportRow> Rows)>(ImportErrors.FailureToReadValue(row));
            }

            rows.Add(new(row, values));
        }

        return (headers, rows);
    }

    private static async Task<Result<(List<string> Headers, List<StagedImportRow> Rows)>> ParseXls(
        MemoryStream stream,
        CancellationToken cancellationToken = default)
    {
        List<string> headers = [];
        List<StagedImportRow> rows = [];

        using IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream);
        DataSet worksheet = reader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
        });

        if (worksheet.Tables.Count == 0 || worksheet.Tables[0].Columns.Count == 0)
            return Result.Failure<(List<string>, List<StagedImportRow>)>(ImportErrors.FileAppearsEmpty);

        int col = 0;
        foreach (DataColumn column in worksheet.Tables[0].Columns)
        {
            col++;
            string rawHeader = column.ColumnName;
            string trimmed = rawHeader.Trim();

            // ExcelDataReader auto-names blank columns as "Column1", "Column2" etc.
            // itself when UseHeaderRow is true — treat that pattern as blank too.
            bool isAutoGenerated = System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^Column\d+$");
            string? effectiveRaw = isAutoGenerated ? null : trimmed;

            if (!isAutoGenerated && headers.Contains(trimmed))
                return Result.Failure<(List<string>, List<StagedImportRow>)>(ImportErrors.DuplicateHeaderValuesDetected);

            headers.Add(ResolveHeaderName(effectiveRaw, col, headers));
        }

        int rowNum = 1; // header is row 1 with UseHeaderRow = true, so data starts at 2

        foreach (DataRow row in worksheet.Tables[0].Rows)
        {
            rowNum++;

            Dictionary<string, string?> values = [];
            
            foreach (string header in headers)
            {
                string? value = row.Field<string>(header);

                bool success = values.TryAdd(header, value);

                if (!success)
                    return Result.Failure<(List<string> Headers, List<StagedImportRow> Rows)>(ImportErrors.FailureToReadValue(rowNum));
            }

            rows.Add(new(rowNum, values));
        }

        return (headers, rows);
    }

    private static async Task<Result<(List<string> Headers, List<StagedImportRow> Rows)>> ParseCsv(
        MemoryStream stream,
        CancellationToken cancellationToken = default)
    {
        List<string> headers = [];
        List<StagedImportRow> rows = [];

        stream.Position = 0;

        using StreamReader textReader = new(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);

        CsvConfiguration config = new(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null, // tolerate short rows rather than throwing
            BadDataFound = null       // tolerate stray unescaped quotes etc. rather than throwing
        };

        using CsvReader csv = new(textReader, config);

        if (!await csv.ReadAsync() || !csv.ReadHeader())
            return Result.Failure<(List<string>, List<StagedImportRow>)>(ImportErrors.FileAppearsEmpty);

        string?[]? headerRow = csv.HeaderRecord;

        if (headerRow is null || headerRow.Length == 0)
            return Result.Failure<(List<string>, List<StagedImportRow>)>(ImportErrors.FileAppearsEmpty);

        int col = 0;
        foreach (string? rawHeader in headerRow)
        {
            col++;
            string trimmed = (rawHeader ?? string.Empty).Trim();

            if (!string.IsNullOrWhiteSpace(trimmed) && headers.Contains(trimmed))
                return Result.Failure<(List<string>, List<StagedImportRow>)>(ImportErrors.DuplicateHeaderValuesDetected);

            headers.Add(ResolveHeaderName(rawHeader, col, headers));
        }

        int rowNum = 1; // header consumed row 1; data starts at row 2

        while (await csv.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();
            rowNum++;

            Dictionary<string, string?> values = [];

            foreach (string header in headers)
            {
                string? value = csv.GetField(header);

                bool success = values.TryAdd(header, value);

                if (!success)
                    return Result.Failure<(List<string>, List<StagedImportRow>)>(ImportErrors.FailureToReadValue(rowNum));
            }

            rows.Add(new(rowNum, values));
        }

        return (headers, rows);
    }

    private static ImportFileFormat DetectFileFormat(Stream stream, string fileName)
    {
        if (!stream.CanSeek)
            throw new ArgumentException("Stream must be seekable to detect file format.", nameof(stream));

        long originalPosition = stream.Position;
        try
        {
            stream.Position = 0;
            byte[] header = new byte[8];
            int bytesRead = stream.Read(header, 0, header.Length);

            if (bytesRead >= 4 && header.AsSpan(0, 4).SequenceEqual(ZipSignature))
                return ImportFileFormat.Xlsx;

            if (bytesRead >= 8 && header.AsSpan(0, 8).SequenceEqual(OleSignature))
                return ImportFileFormat.Xls;

            // No binary signature matched — fall back to extension for CSV,
            // but only trust it if the content actually looks like text.
            if (Path.GetExtension(fileName).Equals(".csv", StringComparison.OrdinalIgnoreCase)
                && LooksLikeText(stream))
            {
                return ImportFileFormat.Csv;
            }

            return ImportFileFormat.Unknown;
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }

    private static bool LooksLikeText(Stream stream)
    {
        stream.Position = 0;
        byte[] buffer = new byte[Math.Min(1024, stream.Length)];
        stream.ReadExactly(buffer, 0, buffer.Length);

        // Reject if there are null bytes or a high proportion of non-printable
        // characters — a decent signal it's binary data with a misleading extension.
        int nonPrintable = buffer.Count(b => b == 0 || (b < 0x09 && b != 0x0A && b != 0x0D));
        return nonPrintable == 0;
    }
    private static string ResolveHeaderName(string? rawHeader, int columnNumber, List<string> existingHeaders)
    {
        string header = (rawHeader ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(header))
            header = $"Column {columnNumber}";

        // Guard against the auto-generated name colliding with a real header
        // elsewhere in the file (e.g. someone genuinely has a column called "Column 7").
        string candidate = header;
        int suffix = 1;
        while (existingHeaders.Contains(candidate))
        {
            suffix++;
            candidate = $"{header} ({suffix})";
        }

        return candidate;
    }
}
