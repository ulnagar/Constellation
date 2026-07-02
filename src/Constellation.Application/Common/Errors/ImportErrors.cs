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


}
