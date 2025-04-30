namespace Constellation.Application.Domains.Attendance.Plans.Commands.CopyAttendancePlanDetails;

using Abstractions.Messaging;
using Core.Models.Attendance.Identifiers;

public sealed record CopyAttendancePlanDetailsCommand(
    AttendancePlanId DestinationPlanId,
    AttendancePlanId SourcePlanId)
    : ICommand;