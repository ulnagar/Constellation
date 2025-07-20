namespace Constellation.Application.Domains.Enrolments.Commands.UnenrolStudentFromOffering;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Students.Identifiers;

public sealed record UnenrolStudentFromOfferingCommand(
    StudentId StudentId,
    OfferingId OfferingId)
    : ICommand;