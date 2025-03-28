namespace Constellation.Application.OfferingEnrolments.GetCurrentEnrolmentsForOffering;

using Constellation.Core.Models.OfferingEnrolments.Identifiers;
using Constellation.Core.Models.Students.Identifiers;

public sealed record EnrolmentResponse(
    EnrolmentId EnrolmentId,
    StudentId StudentId);
