namespace Constellation.Application.Domains.Timetables.Timetables.Queries.GetStaffDailyTimetableData;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;
using System.Collections.Generic;

public sealed record GetStaffDailyTimetableDataQuery(
    StaffId StaffId)
    :IQuery<List<StaffDailyTimetableResponse>>
{
}
