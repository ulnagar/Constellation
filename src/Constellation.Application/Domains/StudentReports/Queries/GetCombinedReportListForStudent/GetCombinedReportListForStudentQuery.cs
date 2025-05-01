namespace Constellation.Application.Domains.StudentReports.Queries.GetCombinedReportListForStudent;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetCombinedReportListForStudentQuery(
    StudentId StudentId)
    : IQuery<List<ReportResponse>>;