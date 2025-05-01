namespace Constellation.Application.Domains.Timetables.Timetables.Queries.GetStaffIntegratedTimetableData;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetStaffIntegratedTimetableDataQuery(
    string StaffId)
    : IQuery<List<StaffIntegratedTimetableResponse>>;