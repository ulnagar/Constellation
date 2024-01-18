namespace Constellation.Application.Students.GetCurrentStudentsWithCurrentOfferings;

using Core.Enums;
using Core.Models.Offerings.Identifiers;
using Core.ValueObjects;
using System.Collections.Generic;

public sealed record StudentWithOfferingsResponse(
    string StudentId,
    Name Name,
    string SchoolName,
    Grade Grade,
    List<StudentWithOfferingsResponse.OfferingResponse> Offerings)
{
    public sealed record OfferingResponse(
        OfferingId Id,
        string Name,
        bool Current);
}