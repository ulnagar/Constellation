namespace Constellation.Application.Domains.EnrolmentContext.Offers.Commands.MarkOfferReviewCompleted;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Offer.Identifiers;

public sealed record MarkOfferReviewCompletedCommand(
    OfferId OfferId,
    string Username)
    : ICommand;