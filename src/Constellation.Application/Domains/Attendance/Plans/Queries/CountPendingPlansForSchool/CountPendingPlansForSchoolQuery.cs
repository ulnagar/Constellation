namespace Constellation.Application.Domains.Attendance.Plans.Queries.CountPendingPlansForSchool;

using Abstractions.Messaging;
using Core.Models.Identifiers;

public sealed record CountPendingPlansForSchoolQuery(
    SchoolCode SchoolCode)
    : IQuery<int>;