namespace Constellation.Application.Enrolments.UnenrolStudentFromAllOfferings;

using Abstractions.Messaging;

public sealed record UnenrolStudentFromAllOfferingsCommand(
    string StudentId)
    : ICommand;
