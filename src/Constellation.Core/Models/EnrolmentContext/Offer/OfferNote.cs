namespace Constellation.Core.Models.EnrolmentContext.Offer;

using Errors;
using Identifiers;
using Shared;

public sealed record OfferNote
{
    // Required by EF Core
    private OfferNote() { }

    private OfferNote(
        OfferId offerId,
        string note,
        string createdBy)
    {
        OfferId = offerId;
        Note = note;
        CreatedBy = createdBy;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public OfferNoteId Id { get; private set; } = new();
    public OfferId OfferId { get; private set; }
    public string Note { get; private set; } = string.Empty;
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }

    public static Result<OfferNote> Create(
        OfferId offerId,
        string note,
        string createdBy)
    {
        if (string.IsNullOrWhiteSpace(note))
            return Result.Failure<OfferNote>(OfferNoteErrors.NoteEmpty);

        OfferNote offerNote = new(
            offerId,
            note,
            createdBy);

        return offerNote;
    }
}