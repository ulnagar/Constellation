namespace Constellation.Application.Domains.EnrolmentContext.Offers.Commands.CreateOfferFromApplication;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Offer.Identifiers;
using ApplicationId = Core.Models.EnrolmentContext.Application.Identifiers.ApplicationId;

public sealed record CreateOfferFromApplicationCommand(
    ApplicationId ApplicationId)
    : ICommand<OfferId>;
