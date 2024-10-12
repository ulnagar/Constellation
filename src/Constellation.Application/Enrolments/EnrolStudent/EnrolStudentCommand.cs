namespace Constellation.Application.Enrolments.EnrolStudent;

using Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;
using Core.Models.Students.Identifiers;

public sealed record EnrolStudentCommand(
    StudentId StudentId,
    OfferingId OfferingId)
    : ICommand;