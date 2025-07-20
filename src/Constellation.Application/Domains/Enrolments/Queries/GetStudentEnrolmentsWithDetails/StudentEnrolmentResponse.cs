namespace Constellation.Application.Domains.Enrolments.Queries.GetStudentEnrolmentsWithDetails;

using Constellation.Core.Models.Offerings.Identifiers;
using Core.Models.Tutorials.Identifiers;
using System.Collections.Generic;

public abstract record StudentEnrolmentResponse(
    string Name,
    string Course,
    List<string> Teachers,
    List<StudentEnrolmentResponse.Resource> Resources,
    bool IsDeleted)
{
    public sealed record Resource(
        string Type,
        string Name,
        string Url);
}

public sealed record StudentOfferingEnrolmentResponse(
    OfferingId OfferingId,
    string OfferingName,
    string CourseName,
    List<string> Teachers,
    List<StudentEnrolmentResponse.Resource> Resources,
    bool IsDeleted) : StudentEnrolmentResponse(OfferingName, CourseName, Teachers, Resources, IsDeleted);

public sealed record StudentTutorialEnrolmentResponse(
    TutorialId TutorialId,
    string TutorialName,
    List<string> Teachers,
    List<StudentEnrolmentResponse.Resource> Resources,
    bool IsDeleted) : StudentEnrolmentResponse(TutorialName, "Tutorial", Teachers, Resources, IsDeleted);