namespace Constellation.Application.OfferingEnrolments.UnenrolStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Students.Identifiers;

public sealed record UnenrolStudentCommand(
    StudentId StudentId,
    OfferingId OfferingId)
    : ICommand;