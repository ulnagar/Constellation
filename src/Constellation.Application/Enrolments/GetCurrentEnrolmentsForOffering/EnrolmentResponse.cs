namespace Constellation.Application.Enrolments.GetCurrentEnrolmentsForOffering;

using Core.Models.Enrolments.Identifiers;

public sealed record EnrolmentResponse(
    EnrolmentId EnrolmentId,
    string StudentId);
