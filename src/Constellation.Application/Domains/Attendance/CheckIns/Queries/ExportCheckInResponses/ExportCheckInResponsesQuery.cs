namespace Constellation.Application.Domains.Attendance.CheckIns.Queries.ExportCheckInResponses;

using Abstractions.Messaging;
using DTOs;
using Queries.GetCheckInResponses;

public sealed record ExportCheckInResponsesQuery(
    CheckInFilter? Filter = null)
    : IQuery<FileDto>;