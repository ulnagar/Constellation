namespace Constellation.Application.Domains.Offerings.Commands.RemoveTeacherFromAllOfferings;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;

public sealed record RemoveTeacherFromAllOfferingsCommand(
    StaffId StaffId)
    : ICommand;