namespace Constellation.Application.Domains.EnrolmentContext.Offers.Commands.DeclineOffer;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Offer.Identifiers;

public sealed record DeclineOfferCommand(
    OfferId OfferId)
    : ICommand;