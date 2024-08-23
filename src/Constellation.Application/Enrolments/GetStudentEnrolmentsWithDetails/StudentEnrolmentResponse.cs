namespace Constellation.Application.Enrolments.GetStudentEnrolmentsWithDetails;

using Constellation.Core.Models.Offerings.Identifiers;
using System.Collections.Generic;

public sealed record StudentEnrolmentResponse(
    OfferingId OfferingId,
    string OfferingName,
    string CourseName,
    List<string> Teachers,
    List<StudentEnrolmentResponse.Resource> Resources,
    bool IsDeleted)
{
    public sealed record Resource(
        string Type,
        string Name,
        string Url);
}