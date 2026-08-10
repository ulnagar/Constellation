namespace Constellation.Application.Domains.EnrolmentContext.Offers.Commands.AcceptOffer;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Offer.Identifiers;

public sealed record AcceptOfferCommand(
    OfferId OfferId,
    bool HasCourtOrders,
    bool HasHealthConditions,
    bool RequestLaptop)
    : ICommand;