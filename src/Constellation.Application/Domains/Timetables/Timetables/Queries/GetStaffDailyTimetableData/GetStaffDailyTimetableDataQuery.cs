namespace Constellation.Application.Domains.Timetables.Timetables.Queries.GetStaffDailyTimetableData;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetStaffDailyTimetableDataQuery(
    string StaffId)
    :IQuery<List<StaffDailyTimetableResponse>>
{
}
