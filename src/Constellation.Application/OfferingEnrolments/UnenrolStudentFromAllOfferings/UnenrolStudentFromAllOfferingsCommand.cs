namespace Constellation.Application.OfferingEnrolments.UnenrolStudentFromAllOfferings;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Students.Identifiers;

public sealed record UnenrolStudentFromAllOfferingsCommand(
    StudentId StudentId)
    : ICommand;
