﻿namespace Constellation.Application.Domains.Attendance.Plans.Queries.GetAttendancePlanForSubmit;

using Abstractions.Messaging;
using Core.Models.Attendance.Identifiers;

public sealed record GetAttendancePlanForSubmitQuery(
    AttendancePlanId PlanId,
    GetAttendancePlanForSubmitQuery.ModeOptions Mode = GetAttendancePlanForSubmitQuery.ModeOptions.View)
    : IQuery<AttendancePlanEntry>
{
    public enum ModeOptions
    {
        View,
        Edit
    }
}