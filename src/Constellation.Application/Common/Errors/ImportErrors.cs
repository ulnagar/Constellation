namespace Constellation.Application.Common.Errors;

using Core.Shared;
using System;

public static class ImportErrors
{
    public static readonly Error InvalidFileSignature = new(
        "Import.Staging.InvalidFileSignature",
        "The provided file is not in a recognisable format");

    public static readonly Error FileAppearsEmpty = new(
        "Import.Staging.FileAppearsEmpty",
        "The provided file does not have any data to import");

    public static readonly Error DuplicateHeaderValuesDetected = new(
        "Import.Staging.DuplicateHeaderValuesDetected",
        "The provided file has more than one column with the same name");

    public static readonly Func<int, Error> FailureToReadValue = row => new(
        "Import.Staging.FailureToReadValue",
        $"Could not read the values in row {row}");

    public static readonly Func<List<string>, Error> InvalidColumnMapping = errors => new(
        "Import.Mapping.InvalidColumnMapping",
        $"Errors found when validating column maps:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");

    public static readonly Func<string, int, Error> RequiredFieldMissing = (column, row) => new(
        "Import.Mapping.RequiredFieldMissing",
        $"Error in row {row}: required field '{column}' does not contain a value");

    public static readonly Func<Type, string, Error> ValueParseError = (type, column) => new(
        "Import.Mapping.ValueParseError",
        $"Value provided for {column} cannot be converted to type {type.Name}");

    public static readonly Error StagedImportExpired = new(
        "Import.Staging.StagedImportExpired",
        "The provided import staging key has expired and the data is no longer available");

    public static readonly Func<string, Error> IncompleteFieldGroup = group => new(
        "Import.Mapping.IncompleteFieldGroup",
        $"The group {group} is missing some required fields");
}

