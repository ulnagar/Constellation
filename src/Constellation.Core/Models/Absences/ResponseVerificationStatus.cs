namespace Constellation.Core.Models.Absences;

using Constellation.Core.Common;

public class ResponseVerificationStatus : StringEnumeration<ResponseVerificationStatus>
{
    public static readonly ResponseVerificationStatus NotRequired = new("NR", "Not Required");
    public static readonly ResponseVerificationStatus Pending = new("Pending", "Pending");
    public static readonly ResponseVerificationStatus Rejected = new("Rejected", "Rejected");
    public static readonly ResponseVerificationStatus Verified = new("Verified", "Verified");

    public ResponseVerificationStatus(string value, string name)
        : base(value, name) { }

    public override string ToString() =>
        Value.ToString();

    public static implicit operator string(ResponseVerificationStatus status) => 
        status is null ? string.Empty : status.ToString();
}