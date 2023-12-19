namespace Constellation.Application.Awards.GetAwardDetailsFromSentral;

using System;

public sealed record AwardDetailResponse(
    string Category,
    string Type,
    DateTime AwardedDate,
    DateTime AwardCreated,
    string Source,
    string SentralStudentId,
    string StudentId,
    string FirstName,
    string LastName);