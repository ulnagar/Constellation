namespace Constellation.Application.Reports.GetAcademicReportList;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetAcademicReportListQuery(
    StudentId StudentId)
    : IQuery<List<AcademicReportResponse>>;