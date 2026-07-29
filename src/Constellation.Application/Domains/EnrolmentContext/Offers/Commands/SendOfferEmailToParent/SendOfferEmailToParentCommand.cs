namespace Constellation.Application.Domains.EnrolmentContext.Offers.Commands.SendOfferEmailToParent;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Offer.Identifiers;

public sealed record SendOfferEmailToParentCommand(
    OfferId OfferId)
    : ICommand;