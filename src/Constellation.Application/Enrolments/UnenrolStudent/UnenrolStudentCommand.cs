namespace Constellation.Application.Enrolments.UnenrolStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Subjects.Identifiers;

public sealed record UnenrolStudentCommand(
    string StudentId,
    OfferingId OfferingId)
    : ICommand;