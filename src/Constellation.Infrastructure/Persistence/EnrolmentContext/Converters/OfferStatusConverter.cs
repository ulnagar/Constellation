namespace Constellation.Infrastructure.Persistence.EnrolmentContext.Converters;

using Core.Models.EnrolmentContext.Offer.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

internal sealed class OfferStatusConverter : ValueConverter<OfferStatus, string>
{
    public OfferStatusConverter()
        : base(
            status => OfferStatusToString(status),
            value => StringToOfferStatus(value),
            new ConverterMappingHints())
    { }

    private static string OfferStatusToString(OfferStatus status) =>
        status.Value;

    private static OfferStatus StringToOfferStatus(string value) =>
        OfferStatus.FromValue(value)!;
}