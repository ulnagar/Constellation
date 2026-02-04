namespace Constellation.Application.Domains.Attendance.CheckIns.Queries.GetCheckInResponses;

using Abstractions.Messaging;
using Core.Enums;
using Core.Models.Attendance.Checkin;
using Core.Models.Offerings.Identifiers;
using Core.Models.Subjects.Identifiers;

public sealed record GetCheckInResponsesQuery(
    Grade? Grade = null,
    string? SchoolCode = null,
    OfferingId? OfferingId = null,
    CourseId? CourseId = null)
    : IQuery<List<CheckInResponse>>;