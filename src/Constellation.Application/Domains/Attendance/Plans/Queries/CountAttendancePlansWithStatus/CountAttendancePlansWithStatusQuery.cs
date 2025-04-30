namespace Constellation.Application.Domains.Attendance.Plans.Queries.CountAttendancePlansWithStatus;

using Abstractions.Messaging;

public sealed record CountAttendancePlansWithStatusQuery()
    : IQuery<(int Pending, int Processing)>;