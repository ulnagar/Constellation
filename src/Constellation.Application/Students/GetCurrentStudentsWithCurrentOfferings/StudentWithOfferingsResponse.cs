namespace Constellation.Application.Students.GetCurrentStudentsWithCurrentOfferings;

using Core.Enums;
using Core.Models.Offerings.Identifiers;
using Core.Models.Students.Enums;
using Core.Models.Students.Identifiers;
using Core.Models.Students.ValueObjects;
using Core.ValueObjects;
using System.Collections.Generic;

public sealed record StudentWithOfferingsResponse(
    StudentId StudentId,
    StudentReferenceNumber StudentReferenceNumber,
    Name Name,
    Gender Gender,
    string SchoolName,
    Grade? Grade,
    List<StudentWithOfferingsResponse.OfferingResponse> Offerings,
    bool CurrentEnrolment)
{
    public sealed record OfferingResponse(
        OfferingId Id,
        string Name,
        bool Current);
}