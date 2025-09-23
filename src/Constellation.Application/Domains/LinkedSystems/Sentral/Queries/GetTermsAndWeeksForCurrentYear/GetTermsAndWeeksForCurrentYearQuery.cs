namespace Constellation.Application.Domains.LinkedSystems.Sentral.Queries.GetTermsAndWeeksForCurrentYear;

using Abstractions.Messaging;
using Attendance.Reports.Queries.GetValidAttendanceReportDates;
using System.Collections.Generic;

public sealed record GetTermsAndWeeksForCurrentYearQuery()
    : IQuery<List<ValidAttendenceReportDate>>, ICommand<List<ValidAttendenceReportDate>>;