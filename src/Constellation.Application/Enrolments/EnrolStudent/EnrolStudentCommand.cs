namespace Constellation.Application.Enrolments.EnrolStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;

public sealed record EnrolStudentCommand(
    string StudentId,
    OfferingId OfferingId)
    : ICommand;