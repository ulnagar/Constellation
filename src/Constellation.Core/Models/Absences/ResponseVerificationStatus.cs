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
}