namespace Constellation.Application.Domains.Timetables.Timetables.Queries.GetStaffIntegratedTimetableData;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;
using System.Collections.Generic;

public sealed record GetStaffIntegratedTimetableDataQuery(
    StaffId StaffId)
    : IQuery<List<StaffIntegratedTimetableResponse>>;