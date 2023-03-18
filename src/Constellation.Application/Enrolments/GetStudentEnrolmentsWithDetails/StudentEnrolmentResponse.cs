namespace Constellation.Application.Enrolments.GetStudentEnrolmentsWithDetails;

using System.Collections.Generic;

public sealed record StudentEnrolmentResponse(
    int OfferingId,
    string OfferingName,
    string CourseName,
    List<string> Teachers,
    bool IsDeleted);