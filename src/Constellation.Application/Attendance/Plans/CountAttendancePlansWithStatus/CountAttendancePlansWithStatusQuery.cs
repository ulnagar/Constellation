namespace Constellation.Application.Attendance.Plans.CountAttendancePlansWithStatus;

using Abstractions.Messaging;

public sealed record CountAttendancePlansWithStatusQuery()
    : IQuery<(int Pending, int Processing)>;