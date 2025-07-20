namespace Constellation.Application.Domains.Students.Queries.GetCurrentStudentsWithCurrentOfferings;

using Constellation.Core.Models.Tutorials.Identifiers;
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
    List<StudentWithOfferingsResponse.EnrolmentResponse> Offerings,
    bool CurrentEnrolment)
{
    public abstract record EnrolmentResponse(
        string Name,
        bool Current);

    public sealed record OfferingEnrolmentResponse(
        OfferingId OfferingId,
        string Name,
        bool Current)
        : EnrolmentResponse(Name, Current);

    public sealed record TutorialEnrolmentResponse(
        TutorialId TutorialId,
        string Name,
        bool Current)
        : EnrolmentResponse(Name, Current);
}