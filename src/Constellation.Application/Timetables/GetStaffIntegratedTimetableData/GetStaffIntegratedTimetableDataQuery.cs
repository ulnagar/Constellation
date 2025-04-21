namespace Constellation.Application.Timetables.GetStaffIntegratedTimetableData;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetStaffIntegratedTimetableDataQuery(
    string StaffId)
    : IQuery<List<StaffIntegratedTimetableResponse>>;