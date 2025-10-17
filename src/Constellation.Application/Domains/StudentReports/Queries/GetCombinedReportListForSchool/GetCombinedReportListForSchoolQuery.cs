namespace Constellation.Application.Domains.StudentReports.Queries.GetCombinedReportListForSchool;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using GetCombinedReportListForStudent;
using System.Collections.Generic;

public sealed record GetCombinedReportListForSchoolQuery(
    string SchoolCode)
    : IQuery<List<SchoolReportResponse>>;