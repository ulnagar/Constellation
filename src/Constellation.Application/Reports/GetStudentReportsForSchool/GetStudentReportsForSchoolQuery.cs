namespace Constellation.Application.Reports.GetStudentReportsForSchool;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetStudentReportsForSchoolQuery(
    string SchoolCode)
    : IQuery<List<SchoolStudentReportResponse>>;