namespace Constellation.Application.Attendance.Plans.CountPendingPlansForSchool;

using Abstractions.Messaging;

public sealed record CountPendingPlansForSchoolQuery(
    string SchoolCode)
    : IQuery<int>;