namespace Constellation.Infrastructure.Persistence.EnrolmentContext.Converters;

using Core.Models.EnrolmentContext.Offer.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

internal sealed class ResponseStatusConverter : ValueConverter<ResponseStatus, string>
{
    public ResponseStatusConverter()
        : base(
            status => ResponseStatusToString(status),
            value => StringToResponseStatus(value),
            new ConverterMappingHints())
    { }

    private static string ResponseStatusToString(ResponseStatus status) =>
        status.Value;

    private static ResponseStatus StringToResponseStatus(string value) =>
        ResponseStatus.FromValue(value)!;
}