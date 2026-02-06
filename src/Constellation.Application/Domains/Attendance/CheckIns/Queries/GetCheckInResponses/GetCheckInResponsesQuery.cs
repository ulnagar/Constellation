namespace Constellation.Application.Domains.Attendance.CheckIns.Queries.GetCheckInResponses;

using Abstractions.Messaging;
using Core.Enums;
using Core.Models.Attendance.Checkin;
using Core.Models.Offerings.Identifiers;
using Core.Models.Subjects.Identifiers;

public sealed record GetCheckInResponsesQuery(
    CheckInFilter? Filter = null)
    : IQuery<List<CheckInResponse>>;