namespace Constellation.Application.Domains.EnrolmentContext.Offers.Commands.MarkOfferApproved;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Offer.Identifiers;

public sealed record MarkOfferApprovedCommand(
    OfferId OfferId,
    string Username)
    : ICommand;