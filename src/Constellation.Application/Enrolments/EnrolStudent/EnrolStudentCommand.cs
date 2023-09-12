namespace Constellation.Application.Enrolments.EnrolStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;

public sealed record EnrolStudentCommand(
    string StudentId,
    OfferingId OfferingId)
    : ICommand;