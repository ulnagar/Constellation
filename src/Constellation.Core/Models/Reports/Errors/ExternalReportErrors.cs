namespace Constellation.Core.Models.Reports.Errors;

using Identifiers;
using Shared;
using System;

public static class ExternalReportErrors
{
    public static readonly Func<ExternalReportId, Error> TempReportNotFound = id => new(
        "Reports.ExternalReport.Temp.NotFound",
        $"A temporary External Report with the Id of {id} could not be found");
}