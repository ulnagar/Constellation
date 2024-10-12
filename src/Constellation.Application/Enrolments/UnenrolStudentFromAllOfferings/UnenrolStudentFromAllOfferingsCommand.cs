namespace Constellation.Application.Enrolments.UnenrolStudentFromAllOfferings;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;

public sealed record UnenrolStudentFromAllOfferingsCommand(
    StudentId StudentId)
    : ICommand;
