namespace Constellation.Application.Domains.Offerings.Commands.RemoveTeacherFromAllOfferings;

using Constellation.Application.Abstractions.Messaging;

public sealed record RemoveTeacherFromAllOfferingsCommand(
    string StaffId)
    : ICommand;