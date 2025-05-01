namespace Constellation.Application.Domains.Enrolments.Commands.UnenrolStudentFromAllOfferings;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;

public sealed record UnenrolStudentFromAllOfferingsCommand(
    StudentId StudentId)
    : ICommand;
