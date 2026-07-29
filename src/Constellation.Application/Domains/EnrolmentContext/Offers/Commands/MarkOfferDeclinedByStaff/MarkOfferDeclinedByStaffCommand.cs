namespace Constellation.Application.Domains.EnrolmentContext.Offers.Commands.MarkOfferDeclinedByStaff;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Offer.Identifiers;

public sealed record MarkOfferDeclinedByStaffCommand(
    OfferId OfferId)
    : ICommand;