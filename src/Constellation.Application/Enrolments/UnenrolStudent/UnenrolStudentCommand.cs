namespace Constellation.Application.Enrolments.UnenrolStudent;

using Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;
using Core.Models.Students.Identifiers;

public sealed record UnenrolStudentCommand(
    StudentId StudentId,
    OfferingId OfferingId)
    : ICommand;