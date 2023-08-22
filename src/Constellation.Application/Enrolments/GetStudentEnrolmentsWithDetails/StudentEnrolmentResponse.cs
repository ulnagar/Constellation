namespace Constellation.Application.Enrolments.GetStudentEnrolmentsWithDetails;

using Constellation.Core.Models.Subjects.Identifiers;
using System.Collections.Generic;

public sealed record StudentEnrolmentResponse(
    OfferingId OfferingId,
    string OfferingName,
    string CourseName,
    List<string> Teachers,
    bool IsDeleted);