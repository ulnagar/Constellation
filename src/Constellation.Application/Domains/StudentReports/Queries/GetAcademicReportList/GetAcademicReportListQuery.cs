namespace Constellation.Application.Domains.StudentReports.Queries.GetAcademicReportList;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetAcademicReportListQuery(
    StudentId StudentId)
    : IQuery<List<AcademicReportResponse>>;