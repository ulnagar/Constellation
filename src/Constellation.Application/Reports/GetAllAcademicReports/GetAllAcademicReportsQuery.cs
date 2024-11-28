namespace Constellation.Application.Reports.GetAllAcademicReports;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAllAcademicReportsQuery()
    : IQuery<List<AcademicReportSummary>>;