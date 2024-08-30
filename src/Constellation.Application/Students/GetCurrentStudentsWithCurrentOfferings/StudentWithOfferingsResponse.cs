namespace Constellation.Application.Students.GetCurrentStudentsWithCurrentOfferings;

using Core.Enums;
using Core.Models.Offerings.Identifiers;
using Core.Models.Students.Enums;
using Core.Models.Students.Identifiers;
using Core.ValueObjects;
using System.Collections.Generic;

public sealed record StudentWithOfferingsResponse(
    StudentId StudentId,
    Name Name,
    Gender Gender,
    string SchoolName,
    Grade Grade,
    List<StudentWithOfferingsResponse.OfferingResponse> Offerings)
{
    public sealed record OfferingResponse(
        OfferingId Id,
        string Name,
        bool Current);
}