namespace Constellation.Application.Domains.EnrolmentContext.Offers.Commands.RecordParentResponseToOffer;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Offer.Enums;
using Core.Models.EnrolmentContext.Offer.Identifiers;

public sealed record RecordParentResponseToOfferCommand(
    OfferId OfferId,
    ResponseStatus Response,
    bool CourtOrder,
    bool HealthConditions,
    bool RequestLaptop)
    : ICommand;