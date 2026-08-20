namespace Constellation.Application.Domains.EnrolmentContext.Offers.Commands.MarkOfferRejected;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Offer.Identifiers;

public sealed record MarkOfferRejectedCommand(
    OfferId OfferId,
    string Username)
    : ICommand;
