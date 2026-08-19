namespace Constellation.Application.Domains.EnrolmentContext.Offers.Commands.AddOfferNote;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Offer.Identifiers;

public sealed record AddOfferNoteCommand(
    OfferId OfferId,
    string Note,
    string CreatedBy)
    : ICommand;