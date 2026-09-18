namespace Constellation.Core.Models.Reports.Errors;

using Constellation.Core.Shared;
using System;

public static class AcademicReportErrors
{
    public static readonly Func<string, Error> NotFoundByPublishId = id => new(
        "Reports.AcademicReport.NotFoundByPublishId",
        $"An academic report with the publish Id of {id} could not be found");

    public static readonly Func<string, Error> FileNotFound = id => new(
        "Reports.AcademicReport.FileNotFound",
        $"Could not find a file with the link id {id}");
}