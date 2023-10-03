namespace Constellation.Application.Offerings.RemoveTeacherFromAllOfferings;

using Constellation.Application.Abstractions.Messaging;

public sealed record RemoveTeacherFromAllOfferingsCommand(
    string StaffId)
    : ICommand;