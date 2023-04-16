namespace Constellation.Application.Reports.GetAcademicReportList;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAcademicReportListQuery(
    string StudentId)
    : IQuery<List<AcademicReportResponse>>;