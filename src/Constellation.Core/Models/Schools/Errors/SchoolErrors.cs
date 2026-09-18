namespace Constellation.Core.Models.Schools.Errors;

using Constellation.Core.Models.Identifiers;
using Shared;
using System;

public static class SchoolErrors
{
    public static readonly Func<SchoolCode, Error> NotFound = id => new(
        "Partners.School.NotFound",
        $"A school with the code {id} could not be found");

    public static readonly Error InvalidValue = new(
        "Partners.School.SchoolCode.InvalidValue",
        "A School Code must be exactly four digits long");
}
