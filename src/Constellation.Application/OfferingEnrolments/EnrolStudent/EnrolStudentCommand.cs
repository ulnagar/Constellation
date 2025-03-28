namespace Constellation.Application.OfferingEnrolments.EnrolStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Students.Identifiers;

public sealed record EnrolStudentCommand(
    StudentId StudentId,
    OfferingId OfferingId)
    : ICommand;