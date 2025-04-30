namespace Constellation.Application.Domains.Attendance.Plans.Queries.CountPendingPlansForSchool;

using Abstractions.Messaging;

public sealed record CountPendingPlansForSchoolQuery(
    string SchoolCode)
    : IQuery<int>;