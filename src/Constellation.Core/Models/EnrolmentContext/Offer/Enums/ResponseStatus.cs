namespace Constellation.Core.Models.EnrolmentContext.Offer.Enums;

using Constellation.Core.Common;

public sealed class ResponseStatus : StringEnumeration<ResponseStatus>
{
    public static readonly ResponseStatus NoResponse = new("No Response");
    public static readonly ResponseStatus Accepted = new("Accepted");
    public static readonly ResponseStatus Declined = new("Declined");

    public ResponseStatus(string value)
    : base(value, value) { }
}