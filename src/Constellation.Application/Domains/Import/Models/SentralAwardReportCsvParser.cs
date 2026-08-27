namespace Constellation.Application.Domains.Import.Models;

using Constellation.Application.Interfaces.Services.Csv;
using Constellation.Core.Shared;
using Errors;
using System;
using System.Collections.Generic;

public sealed class SentralAwardReportCsvParser
{
    private static readonly string[] ExpectedHeaders =
    {
        "award_category", "award_type", "awarded_date", "award_created", "award_source", "student_id", "external_id", "first_name", "surname", "comment"
    };

    private readonly ICsvReader<StudentAwardRow> _reader;

    public SentralAwardReportCsvParser(
        ICsvReader<StudentAwardRow> csvReader)
    {
        _reader = csvReader;
    }

    public Result<List<StudentAwardRow>> Parse(Stream stream) =>
        _reader.Read(
            stream,
            MapRow,
            new CsvReadOptions { HasHeaderRow = true, ExpectedHeaders = ExpectedHeaders });

    private static Result<StudentAwardRow> MapRow(string[] fields)
    {
        if (fields.Length != ExpectedHeaders.Length)
            return Result.Failure<StudentAwardRow>(CsvReaderErrors.FieldCountMismatch(ExpectedHeaders.Length, fields.Length));

        if (!DateOnly.TryParse(fields[2], out var awardedDate))
            return Result.Failure<StudentAwardRow>(CsvReaderErrors.InvalidDate(fields[2]));

        if (!DateTime.TryParse(fields[3], out var createdDate))
            return Result.Failure<StudentAwardRow>(CsvReaderErrors.InvalidDate(fields[3]));

        return Result.Success(new StudentAwardRow(
            fields[0],
            fields[1],
            awardedDate,
            createdDate,
            fields[4],
            fields[5],
            fields[6],
            fields[7],
            fields[8],
            fields[9]));
    }
}

public sealed record StudentAwardRow(
    string Category,
    string Type,
    DateOnly AwardedDate,
    DateTime AwardCreated,
    string AwardSource,
    string StudentId,
    string ExternalId,
    string FirstName,
    string Surname,
    string Comment);

