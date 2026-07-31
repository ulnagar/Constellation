namespace Constellation.Application.Domains.EnrolmentContext.Offers.Commands.MarkOfferPending;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Offer.Identifiers;

public sealed record MarkOfferPendingCommand(
    OfferId OfferId)
    : ICommand;