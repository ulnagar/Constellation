namespace Constellation.Application.Domains.StudentReports.Queries.GetAllAcademicReports;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAllAcademicReportsQuery()
    : IQuery<List<AcademicReportSummary>>;