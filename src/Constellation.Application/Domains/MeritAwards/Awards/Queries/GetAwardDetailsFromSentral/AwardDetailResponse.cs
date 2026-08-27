namespace Constellation.Application.Domains.MeritAwards.Awards.Queries.GetAwardDetailsFromSentral;

using Import.Models;
using System;

public sealed record AwardDetailResponse(
    string Category,
    string Type,
    DateOnly AwardedDate,
    DateTime AwardCreated,
    string Source,
    string SentralStudentId,
    string StudentReferenceNumber,
    string FirstName,
    string LastName)
{
    public static AwardDetailResponse FromCsv(StudentAwardRow row)
    {
        return new AwardDetailResponse(
            row.Category,
            row.Type,
            row.AwardedDate,
            row.AwardCreated,
            row.AwardSource,
            row.StudentId,
            row.ExternalId,
            row.FirstName,
            row.Surname);
    }
}