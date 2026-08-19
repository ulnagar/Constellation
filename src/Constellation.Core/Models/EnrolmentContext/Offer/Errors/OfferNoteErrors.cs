namespace Constellation.Core.Models.EnrolmentContext.Offer.Errors;

using Shared;

public static class OfferNoteErrors
{
    public static readonly Error NoteEmpty = new(
        "Enrolment.OfferNote.NoteEmpty",
        "An Offer Note must contain a message");
}