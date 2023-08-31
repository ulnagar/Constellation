namespace Constellation.Application.Enrolments.UnenrolStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;

public sealed record UnenrolStudentCommand(
    string StudentId,
    OfferingId OfferingId)
    : ICommand;