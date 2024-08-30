namespace Constellation.Application.Awards.GetAwardDetailsFromSentral;

using System;

public sealed record AwardDetailResponse(
    string Category,
    string Type,
    DateTime AwardedDate,
    DateTime AwardCreated,
    string Source,
    string SentralStudentId,
    string StudentReferenceNumber,
    string FirstName,
    string LastName);