namespace Constellation.Core.Models.EnrolmentContext.Offer.Identifiers;

using Primitives;

public readonly record struct OfferNoteId(Guid Value)
    : IStronglyTypedId<OfferNoteId, Guid>
{
    public static OfferNoteId Empty => new(Guid.Empty);

    public static OfferNoteId FromValue(Guid value) =>
        new(value);

    public OfferNoteId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}