namespace Constellation.Application.Domains.EnrolmentContext.Offers.Commands.MarkOfferDocumentsCollected;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Offer.Enums;
using Core.Models.EnrolmentContext.Offer.Identifiers;

public sealed record MarkOfferDocumentsCollectedCommand(
    OfferId OfferId,
    string Username)
    : ICommand;