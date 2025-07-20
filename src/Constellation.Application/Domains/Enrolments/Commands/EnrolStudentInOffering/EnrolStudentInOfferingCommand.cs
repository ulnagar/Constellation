namespace Constellation.Application.Domains.Enrolments.Commands.EnrolStudentInOffering;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Students.Identifiers;

public sealed record EnrolStudentInOfferingCommand(
    StudentId StudentId,
    OfferingId OfferingId)
    : ICommand;