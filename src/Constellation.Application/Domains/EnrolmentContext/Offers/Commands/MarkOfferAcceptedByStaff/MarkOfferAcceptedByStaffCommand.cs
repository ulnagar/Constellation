namespace Constellation.Application.Domains.EnrolmentContext.Offers.Commands.MarkOfferAcceptedByStaff;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Offer.Identifiers;

public sealed record MarkOfferAcceptedByStaffCommand(
    OfferId OfferId,
    bool CourtOrder,
    bool HealthConditions,
    bool RequestLaptop)
    : ICommand;