namespace Constellation.Application.Domains.Import.Errors;

using Core.Shared;
using System.Collections.Generic;

public static class CsvReaderErrors
{
    public static readonly Error EmptyFile =
        new("CsvReader.EmptyFile", "The CSV file was empty");

    public static Error UnexpectedHeaders(string[] expected, string[] actual) =>
        new("CsvReader.UnexpectedHeaders",
            $"Expected headers [{string.Join(", ", expected)}] but found [{string.Join(", ", actual)}]");

    public static Error RowErrors(List<string> errors) =>
        new("CsvReader.RowErrors", string.Join("; ", errors));

    public static readonly Func<int, int, Error> FieldCountMismatch = (expected, found) => new(
        "CsvReader.FieldCountMismatch",
        $"Expected {expected} fields, found {found}");

    public static readonly Func<string, Error> InvalidDate = found => new(
        "CsvReader.InvalidDate",
        $"Could not parse date field '{found}'");
}