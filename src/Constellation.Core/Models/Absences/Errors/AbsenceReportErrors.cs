namespace Constellation.Core.Models.Absences.Errors;

using Shared;

public static class AbsenceReportErrors
{
    public static readonly Error NoFilterSupplied = new(
        "Absences.Report.NoFilterSupplied",
        "Cannot generate report without a supplied filter");
}