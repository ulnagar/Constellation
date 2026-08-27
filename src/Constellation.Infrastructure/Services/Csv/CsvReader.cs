namespace Constellation.Infrastructure.Services.Csv;

using Application.Domains.Import.Errors;
using Application.Interfaces.Services.Csv;
using Core.Shared;
using System;
using System.Collections.Generic;
using System.Text;

internal class CsvReader<T> : ICsvReader<T>
{
    public Result<List<T>> Read(Stream stream, Func<string[], Result<T>> rowMapper, CsvReadOptions? options = null)
    {
        options ??= new CsvReadOptions();
        List<T> results = [];
        List<string> errors = [];

        using StreamReader reader = new StreamReader(stream, options.Encoding);

        if (options.HasHeaderRow)
        {
            string? headerLine = reader.ReadLine();
            if (headerLine is null)
                return Result.Failure<List<T>>(CsvReaderErrors.EmptyFile);

            if (options.ExpectedHeaders is not null)
            {
                var headers = SplitLine(headerLine, options.Delimiter);
                if (!HeadersMatch(headers, options.ExpectedHeaders))
                    return Result.Failure<List<T>>(CsvReaderErrors.UnexpectedHeaders(options.ExpectedHeaders, headers));
            }
        }

        int lineNumber = options.HasHeaderRow ? 1 : 0;

        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            lineNumber++;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] fields = SplitLine(line, options.Delimiter);
            Result<T> mapped = rowMapper(fields);

            if (mapped.IsFailure)
            {
                errors.Add($"Line {lineNumber}: {mapped.Error}");
                continue;
            }

            results.Add(mapped.Value);
        }

        return errors.Count > 0
            ? Result.Failure<List<T>>(CsvReaderErrors.RowErrors(errors))
            : Result.Success(results);
    }

    private static bool HeadersMatch(string[] actual, string[] expected) =>
        actual.Select(h => h.Trim()).SequenceEqual(expected, StringComparer.OrdinalIgnoreCase);

    private static string[] SplitLine(string line, char delimiter)
    {
        // Handles quoted fields containing the delimiter or embedded commas.
        // Swap for a proper CSV library (e.g. CsvHelper) if you hit edge cases
        // like embedded newlines inside quoted fields.
        var fields = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        foreach (char c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == delimiter && !inQuotes)
            {
                fields.Add(current.ToString().Trim().Trim('"'));
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        fields.Add(current.ToString().Trim().Trim('"'));
        return fields.ToArray();
    }
}
